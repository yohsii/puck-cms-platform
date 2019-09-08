using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.En;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace puck.core.Analyzers
{
    public class StemmedEnglishAnalyzer : Analyzer
    {
        protected override TokenStreamComponents CreateComponents(string fieldName,TextReader reader)
        {
            Tokenizer lowerCaseTokenizer = new LowerCaseTokenizer(LuceneVersion.LUCENE_48, reader);

            PorterStemFilter porterStemFilter = new PorterStemFilter(lowerCaseTokenizer);

            StopFilter stopFilter = new StopFilter(LuceneVersion.LUCENE_48,porterStemFilter,EnglishAnalyzer.DefaultStopSet);

            return new TokenStreamComponents(lowerCaseTokenizer, stopFilter);
        }

    }

}
