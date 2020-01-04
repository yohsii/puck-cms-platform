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
using puck.core.State;
namespace puck.core.PuckLucene
{
    public class PuckQueryParser<T>:QueryParser where T : BaseModel
    {
        public static List<string> NumericFieldTypes = new List<string>() { 
            typeof(int).AssemblyQualifiedName,typeof(long).AssemblyQualifiedName,typeof(double).AssemblyQualifiedName,typeof(float).AssemblyQualifiedName
            ,typeof(int?).AssemblyQualifiedName,typeof(long?).AssemblyQualifiedName,typeof(double?).AssemblyQualifiedName,typeof(float?).AssemblyQualifiedName
        };
        private string TypeName = typeof(T).AssemblyQualifiedName;
        public PuckQueryParser(LuceneVersion version, string field, Analyzer analyzer) 
            : base(version, field, analyzer) { 
        }
        protected override Query GetFieldQuery(string field, string queryText, bool quoted) {
            try
            {
                string fieldTypeName = PuckCache.TypeFields[TypeName][field];
                if (fieldTypeName.Equals(typeof(int).AssemblyQualifiedName) || fieldTypeName.Equals(typeof(int?).AssemblyQualifiedName))
                {
                    BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                    NumericUtils.Int32ToPrefixCoded(int.Parse(queryText), 0, bytes);
                    return new TermQuery(new Term(field, bytes));
                }
                else if (fieldTypeName.Equals(typeof(long).AssemblyQualifiedName) || fieldTypeName.Equals(typeof(long?).AssemblyQualifiedName))
                {
                    BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT64);
                    NumericUtils.Int64ToPrefixCoded(long.Parse(queryText), 0, bytes);
                    return new TermQuery(new Term(field, bytes));
                }
                else if (fieldTypeName.Equals(typeof(float).AssemblyQualifiedName) || fieldTypeName.Equals(typeof(float?).AssemblyQualifiedName))
                {
                    BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT32);
                    int intFloat = NumericUtils.SingleToSortableInt32(float.Parse(queryText));
                    NumericUtils.Int32ToPrefixCoded(intFloat, 0, bytes);
                    return new TermQuery(new Term(field, bytes));
                }
                else if (fieldTypeName.Equals(typeof(double).AssemblyQualifiedName) || fieldTypeName.Equals(typeof(double?).AssemblyQualifiedName))
                {
                    BytesRef bytes = new BytesRef(NumericUtils.BUF_SIZE_INT64);
                    long longDouble = NumericUtils.DoubleToSortableInt64(double.Parse(queryText));
                    NumericUtils.Int64ToPrefixCoded(longDouble, 0, bytes);
                    return new TermQuery(new Term(field, bytes));
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
                string fieldTypeName = PuckCache.TypeFields[TypeName][field];
                if (fieldTypeName.Equals(typeof(int).AssemblyQualifiedName) || fieldTypeName.Equals(typeof(int?).AssemblyQualifiedName))
                {
                    return NumericRangeQuery.NewInt32Range(field, int.Parse(part1), int.Parse(part2), inclusiveStart, inclusiveEnd);
                }
                else if (fieldTypeName.Equals(typeof(long).AssemblyQualifiedName) || fieldTypeName.Equals(typeof(long?).AssemblyQualifiedName))
                {
                    return NumericRangeQuery.NewInt64Range(field, long.Parse(part1), long.Parse(part2), inclusiveStart, inclusiveEnd);
                }
                else if (fieldTypeName.Equals(typeof(float).AssemblyQualifiedName) || fieldTypeName.Equals(typeof(float?).AssemblyQualifiedName))
                {
                    return NumericRangeQuery.NewSingleRange(field, float.Parse(part1), float.Parse(part2), inclusiveStart, inclusiveEnd);
                }
                else if (fieldTypeName.Equals(typeof(double).AssemblyQualifiedName) || fieldTypeName.Equals(typeof(double?).AssemblyQualifiedName))
                {
                    return NumericRangeQuery.NewDoubleRange(field, double.Parse(part1), double.Parse(part2), inclusiveStart, inclusiveStart);
                }
            }
            catch (Exception ex) { 
            
            }
            return base.GetRangeQuery(field, part1, part2, inclusiveStart,inclusiveEnd);
        }

    }
}
