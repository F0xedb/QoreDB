using QoreDB.StorageEngine.Index;
using QoreDB.StorageEngine.Index.Interfaces;
using QoreDB.StorageEngine.Index.Nodes;
using Spectre.Console;
using System;
using System.Linq;

namespace QoreDB.Tui.Commands
{
    /// <summary>
    /// Visualizes the B+ Tree for a specific table.
    /// </summary>
    public class BTreeCommand : BaseCommand
    {
        public override string Name => "\\btree";
        public override string[] Aliases => Array.Empty<string>();
        public override string Description => "Visualize the B+ Tree for a table.";

        public override bool Execute(ref Database db, string[] args)
        {
            if (args.Length < 1)
            {
                AnsiConsole.MarkupLine("[red]Error: \\btree command requires a table name.[/]");
                return false;
            }

            var tableName = args[0];
            try
            {
                // We need the concrete type to access the ReadNode method
                var tree = (BackingStorageBPlusTree<int, byte[]>)db.GetTableTree<int>(tableName);
                var root = new Tree($"[purple]B+ Tree for [yellow]{tableName}[/][/]");
                
                BuildBTreeNode(tree, tree.Root, root);
                
                AnsiConsole.Write(root);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            }

            return false;
        }

        /// <summary>
        /// Recursively builds a Spectre.Console tree from B+ Tree nodes with improved visualization.
        /// </summary>
        private void BuildBTreeNode(BackingStorageBPlusTree<int, byte[]> tree, INode<int> node, IHasTreeNodes parent)
        {
            if (node == null) return;

            if (node.IsLeaf)
            {
                var leaf = (LeafNode<int, byte[]>)node;
                var keys = string.Join(", ", leaf.Keys.Select(k => $"[bold]{k}[/]"));
                // Leaf nodes are simple and are rendered as a single line
                parent.AddNode($"[green]Leaf (Page {leaf.PageId})[/]: {keys}");
            }
            else
            {
                var internalNode = (InternalNode<int>)node;
                
                // Create a single, expandable node for the Internal Node
                var internalNodeTreeItem = parent.AddNode($"[blue]Internal Node (Page {internalNode.PageId})[/]");

                // Interleave the child pointers and the keys as children of the node above
                for (int i = 0; i < internalNode.Keys.Count; i++)
                {
                    // Add the child pointer that exists *before* the key
                    var childNode = tree.ReadNode(internalNode.ChildrenPageIds[i]);
                    BuildBTreeNode(tree, childNode, internalNodeTreeItem);

                    // Add the key, which acts as a separator
                    internalNodeTreeItem.AddNode($"[yellow]Key[/]: [bold]{internalNode.Keys[i]}[/]");
                }

                // Add the final child pointer which exists *after* the last key
                var lastChildNode = tree.ReadNode(internalNode.ChildrenPageIds[internalNode.Keys.Count]);
                BuildBTreeNode(tree, lastChildNode, internalNodeTreeItem);
            }
        }
    }
}