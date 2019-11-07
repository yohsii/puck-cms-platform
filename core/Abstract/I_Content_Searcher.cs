using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lucene.Net.Search;
using puck.core.Base;


namespace puck.core.Abstract
{
    public interface I_Content_Searcher
    {
        IList<Dictionary<string, string>> Query(Query query,HashSet<string> fieldsToLoad=null,int limit=500);
        IList<Dictionary<string, string>> Query(string query);
        IList<Dictionary<string, string>> Query(string query,string typeName,HashSet<string> fieldsToLoad=null,int limit=500);
        IList<T> Query<T>(string query) where T:BaseModel;
        IList<T> QueryNoCast<T>(string query) where T:BaseModel;
        IList<T> Query<T>(string query,Filter filter,Sort sort,out int total,int limit,int skip) where T:BaseModel;
        IList<T> QueryNoCast<T>(string query,Filter filter,Sort sort,out int total,int limit,int skip,Type typeOverride=null,bool fallBackToBaseModel=false) where T:BaseModel;
        IList<T> Get<T>(int limit);
        IList<T> Get<T>();
        int Count<T>(string query) where T:BaseModel;
        int DocumentCount();
        public void SetSearcher();
    }
}
