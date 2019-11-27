﻿namespace K2Bridge.Visitors
{
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;
    using K2Bridge.Models.Request.Queries;

    internal interface IVisitor
    {
        void Visit(ExistsQuery existsQuery);

        void Visit(MatchPhraseQuery matchPhraseQuery);

        void Visit(Query query);

        void Visit(ElasticSearchDSL elasticSearchDSL);

        void Visit(RangeQuery rangeQuery);

        void Visit(BoolClause boolClause);

        void Visit(SortClause sortClause);

        void Visit(Aggregation aggregation);

        // Bucket Aggregations
        void Visit(DateHistogram dateHistogram);

        // Metric Aggregations
        void Visit(Avg avg);

        void Visit(Cardinality cardinality);

        void Visit(QueryStringQuery queryStringQuery);
    }
}