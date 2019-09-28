using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using puck.core.Abstract;
using puck.core.Base;
using puck.core.Constants;
using puck.core.Entities;
using puck.core.Models;

namespace puck.core.Abstract
{
    public interface I_Api_Helper
    {
        I_Content_Indexer indexer { get; set; }
        I_Log logger { get; set; }
        I_Puck_Repository repo { get; set; }
        RoleManager<PuckRole> roleManager { get; set; }
        I_Task_Dispatcher tdispatcher { get; set; }
        UserManager<PuckUser> userManager { get; set; }
        void AddTag(string tag, string category);
        void DeleteTag(string tag, string category);
        List<Type> AllModels(bool inclusive = false);
        List<Type> AllowedTypes(string typeName);
        List<FileInfo> AllowedViews(string type, string[] excludePaths = null);
        List<GeneratedProperty> AllProperties(GeneratedModel model);
        List<Variant> AllVariants();
        string DomainMapping(string path);
        List<I_Puck_Editor_Settings> EditorSettings();
        List<string> FieldGroups(string type = null);
        List<GeneratedModel> GeneratedModels();
        List<Type> GeneratedModelTypes(List<Type> excluded = null);
        List<Type> Models(bool inclusive = false);
        Notify NotifyModel(string path);
        List<string> OrphanedTypeNames();
        string PathLocalisation(string path);
        void SetDomain(string path, string domains);
        void SetLocalisation(string path, string variant);
        void SetNotify(Notify model);
        List<BaseTask> SystemTasks();
        List<BaseTask> Tasks();
        List<Type> TaskTypes(bool ignoreSystemTasks = true, bool ignoreBaseTask = true);
        Task<List<PuckUser>> UsersToNotify(string path, NotifyActions action);
        string UserVariant();
        List<Variant> Variants();
        List<FileInfo> Views(string[] excludePaths = null);
    }
}