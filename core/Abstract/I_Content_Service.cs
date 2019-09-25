using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using puck.core.Abstract;
using puck.core.Base;
using puck.core.Entities;

namespace puck.core.Abstract
{
    public interface I_Content_Service
    {
        IConfiguration config { get; set; }
        I_Content_Indexer indexer { get; set; }
        I_Log logger { get; set; }
        I_Puck_Repository repo { get; set; }
        RoleManager<PuckRole> roleManager { get; set; }
        I_Task_Dispatcher tdispatcher { get; set; }
        UserManager<PuckUser> userManager { get; set; }

        void AddAuditEntry(Guid id, string variant, string action, string notes, string username);
        void AddPublishInstruction(List<BaseModel> toIndex);
        Task Copy(Guid id, Guid parentId, bool includeDescendants, string userName = null);
        Task<T> Create<T>(Guid parentId, string variant, string name, string template = null, bool published = false, string userName = null) where T : BaseModel;
        Task Delete(Guid id, string variant = null, string userName = null);
        string GetIdPath(BaseModel mod);
        string GetLiveOrCurrentPath(Guid id);
        Task Move(Guid nodeId, Guid destinationId, string userName = null);
        Task Move(string start, string destination);
        Task Publish(Guid id, string variant, List<string> descendantVariants, string userName = null);
        void Publish(Guid id, string variant, List<string> descendants, bool publish);
        void RenameOrphaned(string orphanTypeName, string newTypeName);
        int RenameOrphaned2(string orphanTypeName, string newTypeName);
        Task RePublishEntireSite();
        Task RePublishEntireSite2();
        Task<List<BaseModel>> SaveContent<T>(T mod, bool makeRevision = true, string userName = null, bool handleNodeNameExists = true, int nodeNameExistsCounter = 0,bool triggerEvents=true,bool shouldIndex=true) where T : BaseModel;
        void Sort(Guid parentId, List<Guid> ids);
        Task UnPublish(Guid id, string variant, List<string> descendantVariants, string userName = null);
        int UpdateDescendantHasNoPublishedRevision(string path, string value, List<string> descendantVariants);
        int UpdateDescendantIdPaths(string oldPath, string newPath);
        int UpdateDescendantIsPublishedRevision(string path, string value, bool addWhereIsCurrentClause, List<string> descendantVariants);
        int UpdateDescendantPaths(string oldPath, string newPath);
        void UpdatePathRelatedMeta(string oldPath, string newPath);
        int UpdateTypeAndTypeChain(string oldType, string newType, string newTypeChain);
    }
}