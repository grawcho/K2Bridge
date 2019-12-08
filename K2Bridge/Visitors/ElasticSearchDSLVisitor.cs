﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Visitors
{
    using System.Collections.Generic;
    using System.Text;
    using K2Bridge.Models.Request;

    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        public void Visit(ElasticSearchDSL elasticSearchDSL)
        {
            var kqlSB = new StringBuilder();

            kqlSB.Append($"{KQLOperators.Let} fromUnixTimeMilli = (t:long) {{datetime(1970 - 01 - 01) + t * 1millisec}};").Append('\n');

            // base query
            elasticSearchDSL.Query.Accept(this);

            // when an index-pattern doesn't have a default time filter the query element can be empty
            var kqlQueryExpression = !string.IsNullOrEmpty(elasticSearchDSL.Query.KQL) ? $"| {elasticSearchDSL.Query.KQL}" : string.Empty;

            kqlSB.Append($"{KQLOperators.Let} _data = {KQLOperators.Materialize}({elasticSearchDSL.IndexName} {kqlQueryExpression});");

            // aggregations
            // TODO: procress the entire list
            if (elasticSearchDSL.Aggregations != null && elasticSearchDSL.Aggregations.Count > 0)
            {
                kqlSB.Append('\n').Append($"(_data | {KQLOperators.Summarize} ");

                foreach (var aggKeyPair in elasticSearchDSL.Aggregations)
                {
                    string name = aggKeyPair.Key;
                    var aggregation = aggKeyPair.Value;
                    aggregation.Accept(this);

                    kqlSB.Append($"{aggregation.KQL} ");
                }

                kqlSB.Append("| as aggs);");
            }

            // hits (projections...)
            kqlSB.Append("\n(_data ");
            if (elasticSearchDSL.Size > 0)
            {
                // we only need to sort if we're returning hits
                var orderingList = new List<string>();

                foreach (var sortClause in elasticSearchDSL.Sort)
                {
                    sortClause.Accept(this);
                    if (!string.IsNullOrEmpty(sortClause.KQL))
                    {
                        orderingList.Add(sortClause.KQL);
                    }
                }

                if (orderingList.Count > 0)
                {
                    kqlSB.Append($"| {KQLOperators.OrderBy} {string.Join(", ", orderingList)} ");
                }
            }

            if (elasticSearchDSL.Size >= 0)
            {
                kqlSB.Append($"| {KQLOperators.Limit} {elasticSearchDSL.Size} | as hits)");
            }

            elasticSearchDSL.KQL = kqlSB.ToString();
        }
    }
}
