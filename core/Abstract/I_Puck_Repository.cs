using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using puck.core.Entities;

namespace puck.core.Abstract
{
    public interface I_Puck_Repository
    {
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

        IQueryable<PuckInstruction> GetPuckInstruction();
        void AddPuckInstruction(PuckInstruction pi);
        void DeletePuckInstruction(PuckInstruction pi);
        IQueryable<PuckAudit> GetPuckAudit();
        void AddPuckAudit(PuckAudit pa);
        void DeletePuckAudit(PuckAudit pa);

        IQueryable<PuckMeta> GetPuckMeta();
        void DeleteMeta(string name, string key, string value);
        void DeleteMeta(PuckMeta meta);
        void AddMeta(PuckMeta meta);
        IQueryable<PuckRevision> GetPuckRevision();
        void DeleteRevision(PuckRevision meta);
        void AddRevision(PuckRevision meta);
        IQueryable<PuckRevision> CurrentRevisionsByPath(string path);
        IQueryable<PuckRevision> CurrentRevisionsByDirectory(string path);
        IQueryable<PuckRevision> CurrentRevisionsByParentId(Guid parentId);
        IQueryable<PuckRevision> CurrentRevisionParent(string path);
        IQueryable<PuckRevision> CurrentRevisionParent(Guid id);
        IQueryable<PuckRevision> CurrentRevisionAncestors(string path);
        List<PuckRevision> CurrentRevisionAncestors(Guid id,bool includeSelf=false);

        IQueryable<PuckRevision> CurrentRevisionDescendants(string idPath);
        IQueryable<PuckRevision> PublishedDescendants(string idPath);
        IQueryable<PuckRevision> CurrentOrPublishedDescendants(string idPath);
        IQueryable<PuckRevision> CurrentRevisionChildren(string path);
        IQueryable<PuckRevision> CurrentRevisionChildren(Guid id);
        IQueryable<PuckRevision> CurrentRevisionVariants(Guid id, string variant);
        IQueryable<PuckRevision> PublishedRevisionVariants(Guid id, string variant);
        IQueryable<PuckRevision> PublishedRevisions(Guid id);
        PuckRevision CurrentRevision(Guid id, string variant);
        PuckRevision PublishedRevision(Guid id,string variant);
        PuckRevision PublishedOrCurrentRevision(Guid id,string variant);
        void SaveChanges();        
    }
}
