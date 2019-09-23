using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Models;
using Lucene.Net.Analysis;
using puck.core.Helpers;
using puck.core.Abstract;
using Lucene.Net.Documents;
using puck.core.Attributes;
using System.Configuration;

namespace puck.core.Constants
{
    public static class GeneratorValues { 
        public static Dictionary<string,GeneratedPropertySelection> PropertyType = new Dictionary<string,GeneratedPropertySelection>(){
            {"SingleLineText",new GeneratedPropertySelection{Name="Single Line Text",Type=typeof(string),AttributeString=""}},
            {"Number",new GeneratedPropertySelection{Name="Number",Type=typeof(int),AttributeString=""}},
            {"PuckPicker",new GeneratedPropertySelection{Name="Puck Picker",Type=typeof(PuckPicker),AttributeString=""}},
            {"RichText",new GeneratedPropertySelection{Name="Rick Text Editor",Type=typeof(string),AttributeString="[UIHint(\"rte\")]"}}
        };
    }
    public enum NotifyActions
    {
        Edit, Publish, Delete, Move
    }
    public static class Mvc {
        public const string AuthenticationScheme = "puck";
    }
    public static class PuckRoles
    {
        public const string Create = "_create";
        public const string Edit = "_edit";
        public const string Delete = "_delete";
        public const string Republish = "_republish";
        public const string Publish = "_publish";
        public const string Unpublish = "_unpublish";
        public const string Revert = "_revert";
        public const string Sort = "_sort";
        public const string Move = "_move";
        public const string Localisation = "_localisation";
        public const string Domain = "_domain";
        public const string Cache = "_cache";
        public const string Notify = "_notify";
        public const string Settings = "_settings";
        public const string Tasks = "_tasks";
        public const string Users = "_users";
        public const string Puck = "_puck";
        public const string Copy = "_copy";
        public const string ChangeType = "_changetype";
        public const string TimedPublish = "_timedpublish";
        public const string Audit = "_audit";
    }
    public static class FieldKeys
    {
        public static string Score = "score";
        public static string Published = "published";
        public static string PuckDefaultField = "";
        public static string PuckValue = "_puckvalue";
        public static string PuckTypeChain = "typechain";
        public static string PuckType = "type";
        public static string ID = "id";
        public static string Path = "path";
        public static string Variant = "variant";
        public static string TemplatePath = "templatepath";
    }
    public static class DBNames
    {
        public static string SyncId = "syncid";
        public static string TypeChain = "typechain";
        public static string TypeAllowedTemplates = "typeallowedtemplates";
        public static string EditorSettings = "editorsettings";
        public static string Redirect301 = "redirect301:";
        public static string Redirect302 = "redirect302:";
        public static string PathToLocale = "pathtolocale";
        public static string Settings = "settings";
        public static string FieldGroups = "fieldgroups:";
        public static string DomainMapping = "domainmapping";
        public static string TypeAllowedTypes = "typeallowedtypes";
        public static string Tasks = "task";
        public static string CachePolicy = "cache";
        public static string CacheExclude = "cacheexclude";
        public static string UserStartNode = "userstartnode";
        public static string UserVariant = "uservariant";
        public static string GeneratedModel = "generatedmodel";
        public static string Notify = "notify";
        public static string TimedPublish = "timedpublish";
        public static string TimedUnpublish = "timedunpublish";
    }
    public static class DBKeys
    {
        //public static string ObjectCacheMinutes = "objectcachemin";
        public static string ObjectCacheMinutes = "objectcachemin";
        public static string Languages = "languages";
        public static string DefaultLanguage = "defaultlanguage";
        public static string EnableLocalePrefix = "enablelocaleprefix";
    }
    public static class InstructionKeys {
        public static string SetSearcher = "setsearcher";
        public static string RepublishSite = "republishsite";
        public static string Publish = "publish";
        public static string RePublish = "republish";
        public static string Delete = "delete";
        public static string UpdateSettings = "updatesettings";
        public static string UpdateCrops = "updatecrops";
        public static string UpdatePathLocales = "updatepathlocales";
        public static string UpdateCacheMappings = "updatecachemappings";
        public static string UpdateDomainMappings = "updatedomainmappings";
        public static string UpdateRedirects = "updateredirects";
        public static string UpdateTaskMappings = "updatetaskmappings";
    }
    public static class CacheKeys {
        public static string PrefixTemplateExist = "fexist:";
    }
    public static class AuditActions {
        public const string Publish = "publish";
        public const string Save = "save";
        public const string Create = "create";
        public const string Move = "move";
        public const string Copy = "copy";
        public const string Delete = "delete";
        public const string Unpublish = "unpublish";
    }
    public static class FieldSettings
    {
        public static Dictionary<Type, Type> DefaultPropertyTransformers = new Dictionary<Type, Type>
        {
            {typeof(DateTime),typeof(DateTransformer)}
        };
        public static Field.Index FieldIndexSetting = Field.Index.ANALYZED;
        public static Field.Store FieldStoreSetting = Field.Store.NO;

    }
    
}
