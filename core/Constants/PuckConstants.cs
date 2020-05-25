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
using puck.core.Attributes.Transformers;
using System.Configuration;
using Microsoft.AspNetCore.Http;

namespace puck.core.Constants
{
    public static class GeneratorValues { 
        public static Dictionary<string,GeneratedPropertySelection> PropertyType = new Dictionary<string,GeneratedPropertySelection>(){
            {"SingleLineText",new GeneratedPropertySelection{Name="Single Line Text",Type=typeof(string),AttributeString=""}},
            {"Number",new GeneratedPropertySelection{Name="Number",Type=typeof(int),AttributeString=""}},
            {"ContentPicker",new GeneratedPropertySelection{Name="Content Picker",Type=typeof(PuckReference),AttributeString=""}},
            {"RichText",new GeneratedPropertySelection{Name="Rick Text Editor",Type=typeof(string),AttributeString=$"[UIHint(\"{puck.core.Constants.EditorTemplates.RichText}\")]"}}
        };
    }
    public enum NotifyActions
    {
        Edit, Publish, Delete, Move
    }
    public static class EditorTemplates {
        public const string ContentPicker = "ContentPicker";
        public const string ImagePicker = "ImagePicker";
        public const string Map = "GeoPosition";
        public const string RichText = "RichText";
        public const string Tags = "Tags";
        public const string ListEditor = "ListEditor";
        public const string TextArea = "TextArea";
        public const string SelectList = "SelectList";
        public const string MultiSelectList = "MultiSelectList";
        public const string ReadOnly = "PuckReadOnly";
    }
    public static class Claims {
        public const string PuckStartId = "PuckStartId";
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
        public const string Sync = "_sync";
        public const string WorkflowDelete = "_workflowdelete";
        public static Dictionary<string, string> NamesDictionary = new Dictionary<string, string> {{Create,"Create"},{Edit,"Edit"},{Delete,"Delete"},{Republish,"Re-publish"},{Publish,"Publish"}
            ,{Unpublish,"Unpublish"},{Revert,"Revert"},{Sort,"Sort"},{Move,"Move" },{Localisation,"Localisation" },{Domain,"Domain" },{Cache,"Cache" },{Notify,"Notify" },{Settings,"Settings" }
            ,{Tasks,"Tasks" },{Users,"Users" },{Puck,"Puck" },{Copy,"Copy" },{ChangeType,"Change Type" },{TimedPublish,"Timed Publish" },{Audit,"Audit" },{Sync,"Sync" },{WorkflowDelete,"Workflow Delete" }
        };
    }
    
    public static class PuckUserGroups {
        public static string[] Administrator = new string[] {
            PuckRoles.Create,
            PuckRoles.Edit,
            PuckRoles.Delete,
            PuckRoles.Republish,
            PuckRoles.Publish,
            PuckRoles.Unpublish,
            PuckRoles.Revert,
            PuckRoles.Sort,
            PuckRoles.Move,
            PuckRoles.Localisation,
            PuckRoles.Domain,
            PuckRoles.Cache,
            PuckRoles.Notify,
            PuckRoles.Settings,
            PuckRoles.Tasks,
            PuckRoles.Users,
            PuckRoles.Puck,
            PuckRoles.Copy,
            PuckRoles.ChangeType,
            PuckRoles.TimedPublish,
            PuckRoles.Audit,
            PuckRoles.Sync
        };
        public static string[] Editor = new string[] {
            PuckRoles.Create,
            PuckRoles.Edit,
            PuckRoles.Delete,
            PuckRoles.Republish,
            PuckRoles.Publish,
            PuckRoles.Unpublish,
            PuckRoles.Revert,
            PuckRoles.Sort,
            PuckRoles.Move,
            PuckRoles.Localisation,
            PuckRoles.Cache,
            PuckRoles.Notify,
            PuckRoles.Puck,
            PuckRoles.Copy,
            PuckRoles.ChangeType,
            PuckRoles.TimedPublish,
            PuckRoles.Audit,
            PuckRoles.Sync
        };
    }
    public static class FieldKeys
    {
        public const string Score = "score";
        public const string Published = "Published";
        public const string PuckDefaultField = "";
        public const string PuckValue = "_puckvalue";
        public const string PuckTypeChain = "TypeChain";
        public const string PuckType = "Type";
        public const string ID = "Id";
        public const string Path = "Path";
        public const string Variant = "Variant";
        public const string TemplatePath = "TemplatePath";
    }
    public static class DBNames
    {
        public const string UserGroup = "usergroup";
        public const string PasswordResetToken = "passwordresettoken";
        public const string SyncId = "syncid";
        public const string TypeChain = "typechain";
        public const string TypeAllowedTemplates = "typeallowedtemplates";
        public const string EditorSettings = "editorsettings";
        public const string Redirect301 = "redirect301:";
        public const string Redirect302 = "redirect302:";
        public const string PathToLocale = "pathtolocale";
        public const string Settings = "settings";
        public const string FieldGroups = "fieldgroups:";
        public const string DomainMapping = "domainmapping";
        public const string TypeAllowedTypes = "typeallowedtypes";
        public const string Tasks = "task";
        public const string CachePolicy = "cache";
        public const string CacheExclude = "cacheexclude";
        public const string UserStartNode = "userstartnode";
        public const string UserVariant = "uservariant";
        public const string GeneratedModel = "generatedmodel";
        public const string Notify = "notify";
        public const string TimedPublish = "timedpublish";
        public const string TimedUnpublish = "timedunpublish";
    }
    public static class DBKeys
    {
        //public static string ObjectCacheMinutes = "objectcachemin";
        public const string ObjectCacheMinutes = "objectcachemin";
        public const string Languages = "languages";
        public const string DefaultLanguage = "defaultlanguage";
        public const string EnableLocalePrefix = "enablelocaleprefix";
    }
    public static class InstructionKeys {
        public const string SetSearcher = "setsearcher";
        public const string RepublishSite = "republishsite";
        public const string Publish = "publish";
        public const string RePublish = "republish";
        public const string Delete = "delete";
        public const string UpdateSettings = "updatesettings";
        public const string UpdateCrops = "updatecrops";
        public const string UpdatePathLocales = "updatepathlocales";
        public const string UpdateCacheMappings = "updatecachemappings";
        public const string UpdateDomainMappings = "updatedomainmappings";
        public const string UpdateRedirects = "updateredirects";
        public const string UpdateTaskMappings = "updatetaskmappings";
        public const string RemoveFromCache = "cacheremove";
    }
    public static class CacheKeys {
        public const string PrefixTemplateExist = "fexist:";
    }
    public static class AuditActions {
        public const string RePublish = "republish";
        public const string Publish = "publish";
        public const string Save = "save";
        public const string Create = "create";
        public const string Move = "move";
        public const string Copy = "copy";
        public const string Delete = "delete";
        public const string Unpublish = "unpublish";
        public const string AddRedirect = "redirect-add";
        public const string DeleteRedirect = "redirect-delete";
    }
    public static class FieldSettings
    {
        public static Dictionary<Type, Type> DefaultPropertyTransformers = new Dictionary<Type, Type>
        {
            {typeof(DateTime),typeof(DateTransformer)}
            ,{typeof(DateTime?),typeof(DateTransformer)}
            ,{typeof(List<PuckReference>),typeof(PuckReferencesTransformer) }
            ,{typeof(IFormFile),typeof(DefaultIFormFileTransformer) }
        };
        public static Field.Index FieldIndexSetting = Field.Index.ANALYZED;
        public static Field.Store FieldStoreSetting = Field.Store.NO;

    }
    
}
