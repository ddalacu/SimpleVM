using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleVM.Compiler
{
    public class VariableDeclaratorNode: INode
    {
        public TokenData Name { get; }

        public VariableDeclaratorNode(TokenData name)
        {
            Name = name;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            builder.Append(Name.GetText(asSpan));
        }
    }
    
    public class LocalDeclarationStatementNode : INode
    {
        public LocalDeclarationNode Declaration { get; }

        public LocalDeclarationStatementNode(LocalDeclarationNode declaration)
        {
            Declaration = declaration;
        }

        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            Declaration.Print(asSpan, builder);
            builder.Append(';');
        }
    }
    
    public class LocalDeclarationNode : INode
    {
        public INode Type { get; }
        public List<INode> Declarators { get; }

        public LocalDeclarationNode(INode type, List<INode> declarators)
        {
            Type = type;
            Declarators = declarators;
        }
        
        public void Print(ReadOnlySpan<char> asSpan, StringBuilder builder)
        {
            Type.Print(asSpan, builder);
            builder.Append(' ');

            for (var index = 0; index < Declarators.Count; index++)
            {
                var declarator = Declarators[index];
                declarator.Print(asSpan, builder);
                if (index != Declarators.Count - 1)
                    builder.Append(", ");
            }
        }
    }
}