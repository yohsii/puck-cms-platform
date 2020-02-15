using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Util;
using puck.core.Base;
using puck.core.Constants;
using puck.core.Helpers;
using puck.core.State;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;

namespace puck.core.PuckLucene
{
    public class PuckQueryParser<T>:QueryParser where T : BaseModel
    {
        public static List<string> NumericFieldTypes = new List<string>() { 
            typeof(int).AssemblyQualifiedName,typeof(long).AssemblyQualifiedName,typeof(double).AssemblyQualifiedName,typeof(float).AssemblyQualifiedName
            ,typeof(int?).AssemblyQualifiedName,typeof(long?).AssemblyQualifiedName,typeof(double?).AssemblyQualifiedName,typeof(float?).AssemblyQualifiedName
        };
        private string TypeName = typeof(T).AssemblyQualifiedName;
        public Sort sort = null;
        List<SortField> sorts = null;
        public static SpatialContext ctx = SpatialContext.GEO;
        public Lucene.Net.Search.Filter filter;
        protected Dictionary<string, Type> FieldTypeMappings { get; set; }
        public PuckQueryParser(LuceneVersion version, string field, Analyzer analyzer,Dictionary<string,Type> fieldTypeMappings=null) 
            : base(version, field, analyzer) {
            this.FieldTypeMappings = fieldTypeMappings;
        }
        protected override Query GetFieldQuery(string field, string queryText, bool quoted) {
            try
            {
                if ((field == "PuckGeoK" && !PuckCache.TypeFields[TypeName].ContainsKey("PuckGeoK"))|| (field == "PuckGeoM" && !PuckCache.TypeFields[TypeName].ContainsKey("PuckGeoM")))
                {
                    var parameters = queryText.Split(',',StringSplitOptions.RemoveEmptyEntries);
                    double longitude;
                    double latitude;
                    int distance;
                    if (parameters.Length == 5 && double.TryParse(parameters[1].Trim(),out longitude) && double.TryParse(parameters[2].Trim(),out latitude) && int.TryParse(parameters[3].Trim(),out distance)) {
                        var qh = new QueryHelper<T>();
                        double radius;
                        
                        if (field == "PuckGeoK")
                            radius = DistanceUtils.EARTH_MEAN_RADIUS_KM;
                        else
                            radius = DistanceUtils.EARTH_MEAN_RADIUS_MI;

                        var distDEG = DistanceUtils.Dist2Degrees(distance,radius);
                        qh.GeoFilter(parameters[0].Trim(),longitude,latitude,distance);
                        this.filter = qh.GetFilter();
                        if (parameters[4] == "asc")
                        {
                            qh.SortByDistanceFromPoint(parameters[0].Trim(), longitude, latitude, desc: false);
                            sort = qh.GetSort();
                        }
                        else if(parameters[4]=="desc"){
                            qh.SortByDistanceFromPoint(parameters[0].Trim(), longitude, latitude, desc: true);
                            sort = qh.GetSort();
                        }
                    }
                }
                else
                {
                    Type fieldType = null;
                    if (FieldTypeMappings != null)
                    {
                        if (FieldTypeMappings.TryGetValue(field, out fieldType))
                        {

                        }
                        else
                        {
                            PuckCache.TypeFields[TypeName]?.TryGetValue(field, out fieldType);
                        }
                    }
                    else
                    {
                        PuckCache.TypeFields[TypeName]?.TryGetValue(field, out fieldType);
                    }
                    if (fieldType != null)
                    {
                        if (fieldType.Equals(typeof(int)) || fieldType.Equals(typeof(int?)))
                        {
                            BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                            NumericUtils.Int32ToPrefixCoded(int.Parse(queryText), 0, bytes);
                            return new TermQuery(new Term(field, bytes));
                        }
                        else if (fieldType.Equals(typeof(long)) || fieldType.Equals(typeof(long?)))
                        {
                            BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT64);
                            NumericUtils.Int64ToPrefixCoded(long.Parse(queryText), 0, bytes);
                            return new TermQuery(new Term(field, bytes));
                        }
                        else if (fieldType.Equals(typeof(float)) || fieldType.Equals(typeof(float?)))
                        {
                            BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                            int intFloat = NumericUtils.SingleToSortableInt32(float.Parse(queryText));
                            NumericUtils.Int32ToPrefixCoded(intFloat, 0, bytes);
                            return new TermQuery(new Term(field, bytes));
                        }
                        else if (fieldType.Equals(typeof(double)) || fieldType.Equals(typeof(double?)))
                        {
                            BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT64);
                            long longDouble = NumericUtils.DoubleToSortableInt64(double.Parse(queryText));
                            NumericUtils.Int64ToPrefixCoded(longDouble, 0, bytes);
                            return new TermQuery(new Term(field, bytes));
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return base.GetFieldQuery(field, queryText,quoted);
        }
        protected override Query GetRangeQuery(string field, string part1, string part2, bool inclusiveStart,bool inclusiveEnd)
        {
            try
            {
                Type fieldType = null;
                if (FieldTypeMappings != null)
                {
                    if (FieldTypeMappings.TryGetValue(field, out fieldType))
                    {
                        
                    }
                    else
                    {
                        PuckCache.TypeFields[TypeName]?.TryGetValue(field, out fieldType);
                    }
                }
                else
                {
                    PuckCache.TypeFields[TypeName]?.TryGetValue(field, out fieldType);
                }
                if (fieldType!=null)
                {
                    if (fieldType.Equals(typeof(int)) || fieldType.Equals(typeof(int?)))
                    {
                        return NumericRangeQuery.NewInt32Range(field, int.Parse(part1), int.Parse(part2), inclusiveStart, inclusiveEnd);
                    }
                    else if (fieldType.Equals(typeof(long)) || fieldType.Equals(typeof(long?)))
                    {
                        return NumericRangeQuery.NewInt64Range(field, long.Parse(part1), long.Parse(part2), inclusiveStart, inclusiveEnd);
                    }
                    else if (fieldType.Equals(typeof(float)) || fieldType.Equals(typeof(float?)))
                    {
                        return NumericRangeQuery.NewSingleRange(field, float.Parse(part1), float.Parse(part2), inclusiveStart, inclusiveEnd);
                    }
                    else if (fieldType.Equals(typeof(double)) || fieldType.Equals(typeof(double?)))
                    {
                        return NumericRangeQuery.NewDoubleRange(field, double.Parse(part1), double.Parse(part2), inclusiveStart, inclusiveStart);
                    }
                }
            }
            catch (Exception ex) { 
            
            }
            return base.GetRangeQuery(field, part1, part2, inclusiveStart,inclusiveEnd);
        }

    }
}
