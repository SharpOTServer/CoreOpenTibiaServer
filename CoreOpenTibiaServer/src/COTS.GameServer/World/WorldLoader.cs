﻿using System;
using System.Collections.Generic;
using System.IO;

namespace COTS.GameServer.World {

    public static class WorldLoader {

        /// <summary>
        /// Array of 'ascii bytes' ['O', 'T', 'B', 'M'] or ['\0', '\0', '\0', '\0']
        /// </summary>
        private const int IdentifierLength = 4;

        /// <summary>
        /// NodeStart + NodeType + NodeEnd
        /// </summary>
        private const int MinimumNodeSize = 3;

        private const int MinimumWorldSize = IdentifierLength + MinimumNodeSize;

        public static World ParseWorld(byte[] serializedWorldData) {
            if (serializedWorldData == null)
                throw new ArgumentNullException(nameof(serializedWorldData));
            if (serializedWorldData.Length < MinimumWorldSize)
                throw new MalformedWorldException();

            using (var stream = new MemoryStream(serializedWorldData)) {
                var rootNode = ParseTree(stream);
                var world = new World(
                    root: rootNode,
                    serializedWorldData: serializedWorldData);

                return world;
            }
        }

        private static WorldNode ParseTree(MemoryStream stream) {
            // Skipping the first 4 bytes coz they are used to store a... identifier?
            stream.Seek(IdentifierLength, SeekOrigin.Begin);

            var firstMarker = (WorldNode.NodeMarker)stream.ReadByte();
            if (firstMarker != WorldNode.NodeMarker.Start)
                throw new MalformedWorldException();

            var guessedMaximumNodeDepth = 128;
            var nodeStack = new Stack<WorldNode>(capacity: guessedMaximumNodeDepth);

            var rootNodeType = (byte)stream.ReadByte();
            var rootNode = new WorldNode() {
                Type = rootNodeType,
                PropsBegin = (int)stream.Position + 1
            };
            nodeStack.Push(rootNode);

            ParseTreeAfterRootNodeStart(stream, nodeStack);
            if (nodeStack.Count != 0)
                throw new MalformedWorldException();

            return rootNode;
        }

        private static void ParseTreeAfterRootNodeStart(
            MemoryStream stream,
            Stack<WorldNode> nodeStack
            ) {
            while (true) {
                var currentInt = stream.ReadByte();
                if (currentInt == -1)
                    return;

                var currentByte = (byte)currentInt;
                var currentMark = (WorldNode.NodeMarker)currentByte;
                switch (currentMark) {
                    case WorldNode.NodeMarker.Start:
                    ProcessNodeStart(stream, nodeStack);
                    break;

                    case WorldNode.NodeMarker.End:
                    ProcessNodeEnd(stream, nodeStack);
                    break;

                    case WorldNode.NodeMarker.Escape:
                    ProcessNodeEscape(stream);
                    break;

                    default:
                    /// If it's not a <see cref="ParsingWorldNode.NodeMarker"/>, then it's just prop data
                    /// and we can safely skip it.
                    break;
                }
            }
        }

        private static void ProcessNodeStart(MemoryStream stream, Stack<WorldNode> nodeStack) {
            if (!nodeStack.TryPeek(out var currentNode))
                throw new MalformedWorldException();

            if (currentNode.Children.Count == 0)
                currentNode.PropsEnd = (int)stream.Position;

            var childType = stream.ReadByte();
            if (childType == -1)
                throw new MalformedWorldException();

            var child = new WorldNode {
                Type = (byte)childType,
                PropsBegin = (int)stream.Position + sizeof(WorldNode.NodeMarker)
            };

            currentNode.Children.Add(child);
            nodeStack.Push(child);
        }

        private static void ProcessNodeEnd(MemoryStream stream, Stack<WorldNode> nodeStack) {
            if (!nodeStack.TryPeek(out var currentNode))
                throw new MalformedWorldException();

            if (currentNode.Children.Count == 0)
                currentNode.PropsEnd = (int)stream.Position;

            nodeStack.Pop();
        }

        private static void ProcessNodeEscape(MemoryStream stream) {
            var escapedByte = stream.ReadByte();
            if (stream.Position == stream.Length)
                throw new MalformedWorldException();
        }
    }
}