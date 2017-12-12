﻿using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using Fame;
using FAMIX;
using Microsoft.CodeAnalysis;
using RoslynMonoFamix.InCSharp;

public class ModelVisitor : CSharpSyntaxWalker
{
    private SemanticModel semanticModel;
    private InCSharpImporter importer;
    private Method currentMethod;
    private String currentClassKey;

    public ModelVisitor(SemanticModel semanticModel, InCSharpImporter importer)
    {
        this.semanticModel = semanticModel;
        this.importer = importer;
    }

    public override void VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        var typeSymbol = semanticModel.GetDeclaredSymbol(node);
        currentClassKey = FullTypeName(typeSymbol);
        FAMIX.Type type = importer.EnsureType(currentClassKey, typeSymbol);
        type.name = node.Identifier.ToString();
        base.VisitClassDeclaration(node);
    }

    public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        string methodName = node.Identifier.ToString();
        var methodSymbol = semanticModel.GetDeclaredSymbol(node);
        String fullMethodName = FullMethodName(methodSymbol);
        Method aMethod = importer.EnsureMethod(fullMethodName, methodSymbol);
        currentMethod = aMethod;
        base.VisitMethodDeclaration(node);
        currentMethod = null;
    }

    public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        foreach (var variable in node.Declaration.Variables)
        {
            string attributeName = variable.Identifier.ToString();
            var symbol =  semanticModel.GetDeclaredSymbol(variable);
            if (symbol is IFieldSymbol)
            {
                FAMIX.Attribute anAttribute = importer.EnsureAttribute(currentClassKey + "." + attributeName, (IFieldSymbol) symbol);
            }
            
        }
        base.VisitFieldDeclaration(node);
    }

    public T GetSymbol<T>(SyntaxNode node)
    {
        var symbolInfo = semanticModel.GetSymbolInfo(node).Symbol;
        if (symbolInfo is T methodSymbol)
            return methodSymbol;
        return default(T);
    }
    /*
    public override void VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        var symbol = semanticModel.GetDeclaredSymbol(node);
        var expr = node.Expression;
        IMethodSymbol methodSymbol = null;

        if (expr is IdentifierNameSyntax)
        {
            IdentifierNameSyntax identifiername = expr as IdentifierNameSyntax; // identifiername is your method name
            methodSymbol = GetSymbol<IMethodSymbol>(node);
            Console.WriteLine("\t\t\t ID from usage:" + methodSymbol.GetHashCode());
        }
        if (expr is MemberAccessExpressionSyntax)
        {
            MemberAccessExpressionSyntax memberAccess = expr as MemberAccessExpressionSyntax;
            methodSymbol = GetSymbol<IMethodSymbol>(node);
        }

        if (methodSymbol != null && currentMethod != null)
        {
            var calledMethod = importer.EnsureMethod(FullMethodName(methodSymbol), methodSymbol);
            // add the call to currentMethod here (once we have the model created)
            currentMethod.InvokedMethods.Add(calledMethod);
            calledMethod.InvokingMethods.Add(calledMethod);

            Console.WriteLine("\t\t\t Current method:" + currentMethod);
            Console.WriteLine("\t\t\t Calling:" + methodSymbol.Name);
            Console.WriteLine("\t\t\t ID from usage:" + methodSymbol.GetHashCode());
        }
        base.VisitInvocationExpression(node);
    }
    */
    /**
     * Visit an identifier, it can be anything, a method reference, a field, a local variable, etc.
     * We need to find out what it is and if it is located in a method and then make the appropriate connection.
     */
    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (currentMethod != null)
        {
            Console.WriteLine("node+++ " + node.Identifier);
            FAMIX.NamedEntity referencedEntity = FindReferencedEntity(node);
            if (referencedEntity is FAMIX.Method) AddMethodCall(node, currentMethod, referencedEntity as FAMIX.Method);
            if (referencedEntity is FAMIX.Attribute) AddAttributeAccess(currentMethod, referencedEntity as FAMIX.Attribute);
        }
        base.VisitIdentifierName(node);
    }

    private void AddAttributeAccess(Method clientMethod, FAMIX.Attribute attribute)
    {
        Access access = importer.CreateNewAssociation<Access>("FAMIX.Access");
        access.accessor = currentMethod;
        access.variable = attribute;
        clientMethod.AddAccesse(access);
        attribute.AddAccessor(clientMethod);
    }

    private void AddMethodCall(IdentifierNameSyntax node, Method clientMethod, Method referencedEntity)
    {
        Invocation invocation = importer.CreateNewAssociation<Invocation>("FAMIX.Invocation");
        invocation.sender = clientMethod;
        invocation.AddCandidate(referencedEntity);
        invocation.signature = node.Span.ToString();
        clientMethod.AddOutgoingInvocation(invocation);
        referencedEntity.AddIncomingInvocation(invocation);
    }

    private NamedEntity FindReferencedEntity(IdentifierNameSyntax node)
    {
        var symbol = semanticModel.GetSymbolInfo(node).Symbol;
        if (symbol is IMethodSymbol)
            return importer.EnsureMethod(FullMethodName(symbol as IMethodSymbol), symbol as IMethodSymbol);
        if (symbol is IFieldSymbol)
            return importer.EnsureAttribute(currentClassKey + "." + symbol.Name, symbol as IFieldSymbol);
        return null;
    }

    private String FullMethodName(IMethodSymbol method)

    {
        var parameters = "(";
        foreach (var par in method.Parameters)
            parameters += par.Type.Name + ",";
        if (parameters.LastIndexOf(",") > 0)
            parameters = parameters.Substring(0, parameters.Length - 1);
        parameters += ")";

        return currentClassKey + "." + method.Name + parameters;
    }
    private String FullTypeName(INamedTypeSymbol aType)
    {
        String name = aType.Name;
        var ns = aType.ContainingNamespace;
        while (ns != null)
        {
            name = ns.Name + "." + name;
            ns = ns.ContainingNamespace;
        }
        return name;
    }

}