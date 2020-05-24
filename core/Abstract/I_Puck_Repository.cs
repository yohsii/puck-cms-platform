using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Entities;

namespace puck.core.Abstract
{
    public interface I_Puck_Repository
    {
        I_Puck_Context Context { get; }
        IQueryable<GeneratedModel> GetGeneratedModel();
        IQueryable<GeneratedProperty> GetGeneratedProperty();
        IQueryable<GeneratedAttribute> GetGeneratedAttribute();
        IQueryable<PuckUser> GetPuckUser();
        void AddGeneratedModel(GeneratedModel gm);
        void AddGeneratedProperty(GeneratedProperty gm);
        void AddGeneratedAttribute(GeneratedAttribute gm);
        void DeleteGeneratedModel(GeneratedModel gm);
        void DeleteGeneratedProperty(GeneratedProperty gm);
        void DeleteGeneratedAttribute(GeneratedAttribute gm);
        IQueryable<PuckTag> GetPuckTag();
        void AddPuckTag(PuckTag tag);
        void DeletePuckTag(PuckTag tag);

        IQueryable<PuckRedirect> GetPuckRedirect();
        void AddPuckRedirect(PuckRedirect pr);
        void DeletePuckRedirect(PuckRedirect pr);
        IQueryable<PuckWorkflowItem> GetPuckWorkflowItem();
        void AddPuckWorkflowItem(PuckWorkflowItem wfi);
        void DeletePuckWorkflowItem(PuckWorkflowItem wfi);
        IQueryable<PuckInstruction> GetPuckInstruction();
        void AddPuckInstruction(PuckInstruction pi);
        void DeletePuckInstruction(PuckInstruction pi);
        IQueryable<PuckAudit> GetPuckAudit();
        void AddPuckAudit(PuckAudit pa);
        void DeletePuckAudit(PuckAudit pa);

        IQueryable<PuckMeta> GetPuckMeta();
        void DeletePuckMeta(string name, string key, string value);
        void DeletePuckMeta(PuckMeta meta);
        void AddPuckMeta(PuckMeta meta);
        IQueryable<PuckRevision> GetPuckRevision();
        void DeletePuckRevision(PuckRevision revision);
        void AddPuckRevision(PuckRevision revision);
        IQueryable<PuckRevision> CurrentRevisionsByPath(string path);
        IQueryable<PuckRevision> CurrentRevisionsByDirectory(string path);
        IQueryable<PuckRevision> CurrentRevisionsByParentId(Guid parentId);
        IQueryable<PuckRevision> CurrentRevisionParent(string path);
        IQueryable<PuckRevision> CurrentRevisionParent(Guid id);
        IQueryable<PuckRevision> CurrentRevisionAncestors(string path);
        List<PuckRevision> CurrentRevisionAncestors(Guid id,bool includeSelf=false);
        IQueryable<PuckRevision> CurrentRevisionDescendantsByPath(string path);
        IQueryable<PuckRevision> PublishedDescendantsByPath(string path);
        IQueryable<PuckRevision> PublishedOrCurrentDescendantsByPath(string path);
        IQueryable<PuckRevision> CurrentRevisionDescendants(string idPath);
        IQueryable<PuckRevision> PublishedDescendants(string idPath);
        IQueryable<PuckRevision> PublishedOrCurrentDescendants(string idPath);
        IQueryable<PuckRevision> CurrentRevisionChildren(string path);
        IQueryable<PuckRevision> CurrentRevisionChildren(Guid id);
        IQueryable<PuckRevision> CurrentRevisionVariants(Guid id, string variant);
        IQueryable<PuckRevision> PublishedRevisionVariants(Guid id, string variant);
        IQueryable<PuckRevision> CurrentRevisions(Guid id);
        IQueryable<PuckRevision> PublishedRevisions(Guid id);
        PuckRevision CurrentRevision(Guid id, string variant);
        PuckRevision PublishedRevision(Guid id,string variant);
        PuckRevision PublishedOrCurrentRevision(Guid id,string variant);
        IQueryable<PuckRevision> PublishedOrCurrentRevisions(Guid id);
        void SaveChanges();        
    }
}
