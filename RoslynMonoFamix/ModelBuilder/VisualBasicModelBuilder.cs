﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FAMIX;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace RoslynMonoFamix.ModelBuilder {
    class VisualBasicModelBuilder : AbstractModelBuilder {
        FAMIX.ScopingEntity entity;
        public VisualBasicModelBuilder(Fame.Repository repository, string projectBaseFolder) : base(repository, projectBaseFolder) {

        }
        public FAMIX.Inheritance CreateInheritanceFor(FAMIX.Class inheritingClass) {
            FAMIX.Inheritance inheritance = this.CreateNewEntity<FAMIX.Inheritance>(typeof(FAMIX.Inheritance).FullName);
            inheritance.subclass = inheritingClass;
            return inheritance;
        }
        internal ScopingEntity CreateScopingEntity(CompilationUnitSyntax node) {
            if (entity != null) throw new System.Exception(" NOOOOOOO ");
            entity = new FAMIX.ScopingEntity();
            return entity;
        }

        public FAMIX.Class EnsureClass(INamedTypeSymbol type) {
            string classname = helper.FullTypeName(type);
            return Types.EntityNamedIfNone<FAMIX.Class>(classname,
                 () => { return this.CreateNewClass(type); });
        }

        public FAMIX.Class EnsureIterface(InterfaceStatementSyntax node) {
            string classname = helper.FullTypeName(model.GetDeclaredSymbol(node));
            return Types.EntityNamedIfNone<FAMIX.Class>(classname,
                 () => { return this.CreateNewInterface(node); });
        }
        public FAMIX.Namespace EnsureNamespace(INamespaceSymbol ns) {

            //while (Current != null) {
            //    if (Current is NamespaceStatementSyntax) {
            //        NamespaceName = ((NamespaceStatementSyntax)Current).Name.ToFullString() + "." + NamespaceName;
            //    }
            //    Current = Current.Parent;
            //}
            return Namespaces.EntityNamedIfNone<FAMIX.Namespace>(ns.Name,
                 () => { return this.CreateNamespace(ns); });
        }

        public Method EnsureMethod(IMethodSymbol method) {
            string name = helper.FullTypeName(method);
            return Methods.EntityNamedIfNone<FAMIX.Method>(name,
                 () => { return this.CreateNewMethod(method); });
        }

        private Method CreateNewMethod(IMethodSymbol method) {
            Method FamixMethod = this.CreateNewEntity<FAMIX.Method>(typeof(FAMIX.Method).FullName);
            FamixMethod.isStub = true;
            FamixMethod.name = method.Name;
            FamixMethod.signature = helper.MethodSignature(method);
            return FamixMethod;
        }
        private Method CreateNewConstructor(IMethodSymbol method) {
            Method FamixMethod = this.CreateNewMethod(method);
            FamixMethod.isConstructor = true;
            return FamixMethod;
        }

        public Method EnsureConstructor(IMethodSymbol method) {
            string name = helper.FullTypeName(method);
            return Methods.EntityNamedIfNone<FAMIX.Method>(name,
                 () => { return this.CreateNewConstructor(method); });
        }

        private FAMIX.Class CreateNewClass(INamedTypeSymbol type) {
            FAMIX.Class entity = this.CreateNewEntity<FAMIX.Class>(typeof(FAMIX.Class).FullName);
            entity.name = helper.FullTypeName(type);
            entity.isAbstract = type.IsAbstract;
            entity.isFinal = type.IsSealed;
            entity.accessibility = helper.AccessibilityName(type.DeclaredAccessibility);
            return entity;
        }


        private Namespace CreateNamespace(INamespaceSymbol ns) {
            FAMIX.Namespace entity = this.CreateNewEntity<FAMIX.Namespace>(typeof(FAMIX.Namespace).FullName);
            entity.name = ns.Name;
            return entity;
        }

        private FAMIX.Class CreateNewInterface(InterfaceStatementSyntax node) {
            FAMIX.Class entity = this.CreateNewEntity<FAMIX.Class>(helper.FullTypeName(model.GetDeclaredSymbol(node)));
            entity.isInterface = true;
            entity.isAbstract = true;
            return entity;
        }


        public FAMIX.Type EnsureType(ISymbol aType) {

            string fullName = helper.FullTypeName(aType);

            if (Types.has(fullName))
                return Types.Named(fullName);

            string typeKind = helper.ResolveFAMIXTypeName(aType).FullName;

            FAMIX.Type type = repository.New<FAMIX.Type>(typeKind);
            type.isStub = true;

            Types.Add(fullName, type);

            if (typeKind.Equals(typeof(FAMIX.ParameterizedType).FullName)) {
                var parameterizedClass = EnsureType(aType.OriginalDefinition);
                (type as FAMIX.ParameterizedType).parameterizableClass = parameterizedClass as FAMIX.ParameterizableClass;
            }

            type.name = helper.TypeName(aType);
            if (aType.ContainingType != null) {
                var containingType = EnsureType(aType.ContainingType);
                type.container = containingType;
            } else
            if (aType.ContainingNamespace != null) {
                var ns = EnsureNamespace(aType.ContainingNamespace);
                type.container = ns;
            }

            return type;

        }
    }
}
