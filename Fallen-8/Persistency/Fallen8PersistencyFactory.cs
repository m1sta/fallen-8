// 
// Fallen8PersistencyFactory.cs
//  
// Author:
//       Henning Rauch <Henning@RauchEntwicklung.biz>
// 
// Copyright (c) 2012 Henning Rauch
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Generic;
using Fallen8.API.Model;
using Fallen8.API.Index;
using System.IO;
using Fallen8.API.Helper;
using Framework.Serialization;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Fallen8.API.Persistency
{
    /// <summary>
    /// Fallen8 persistency factory.
    /// </summary>
    public static class Fallen8PersistencyFactory
    {
        #region public methods
        
        /// <summary>
        /// Save the specified graphElements, indices and path.
        /// </summary>
        /// <param name='graphElements'>
        /// Graph elements.
        /// </param>
        /// <param name='indices'>
        /// Indices.
        /// </param>
        /// <param name='path'>
        /// Path.
        /// </param>
        public static void Save(Int32 currentId, List<AGraphElement> graphElements, IDictionary<String, IIndex> indices, String path)
        {
            // Create the new, empty data file.
            if (File.Exists(path))
            {
                //the newer save gets an timestamp
                path = path + DateTime.Now.ToBinary().ToString();
            }
            
            var file = File.Create(path, Constants.BufferSize, FileOptions.SequentialScan);
            SerializationWriter writer = null;
           
            writer = new SerializationWriter(file);
            
            //the maximum id
            writer.WriteOptimized(currentId);
            
            #region graph elements
            
            //the number of maximum graph elements
            writer.WriteOptimized(graphElements.Count);
            
            List<String> fileStreamNames = new List<String>();
            var partitions = Partitioner.Create(0, graphElements.Count);
            Parallel.ForEach(
                partitions,
                () => String.Empty,
                (range, loopstate, initialValue) =>
                    {
                        String partitionFileName = path + "_" + range.Item1 + "_to_" + range.Item2;
                
                        //create file for range
                        var partitionFile = File.Create(partitionFileName, Constants.BufferSize, FileOptions.SequentialScan);
                        SerializationWriter partitionWriter = new SerializationWriter(partitionFile);
                        
                        for (int i = range.Item1; i < range.Item2; i++) 
                        {
                            var aGraphElement = graphElements[i];
                    
                            //there can be nulls
                            if (aGraphElement == null) 
                            {
                                writer.WriteObject (null);
                                continue;
                            }
                            
                            //code if it is an vertex or an edge
                            if (aGraphElement is VertexModel) 
                            {
                                WriteVertex((VertexModel)aGraphElement, partitionWriter);
                            }
                            else
                            {
                                WriteEdge((EdgeModel)aGraphElement, partitionWriter);
                            }
                        }
                
                        if (partitionWriter != null) 
                        {
                            partitionWriter.Flush();
                            partitionWriter.Close();
                        }
            
                        if (partitionFile != null) 
                        {
                            partitionFile.Flush();
                            partitionFile.Close();
                        }
                        
                        return partitionFileName;

                    },
                delegate(String rangeFileStream)
                    {
                        lock (fileStreamNames)
                        {
                            fileStreamNames.Add(rangeFileStream);
                        }
                    });
            
            writer.WriteOptimized(fileStreamNames.Count);
            foreach (var aFileStreamName in fileStreamNames) 
            {
                writer.WriteOptimized(aFileStreamName);    
            }
            
            #endregion
            
            if (writer != null) {
                writer.Flush();
                writer.Close();
            }
            
            if (file != null) {
                file.Flush();
                file.Close();
            }
        }
  
        #endregion
        
        #region private helper
        
        /// <summary>
        /// Writes the vertex.
        /// </summary>
        /// <param name='vertex'>
        /// Vertex.
        /// </param>
        /// <param name='writer'>
        /// Writer.
        /// </param>
        private static void WriteVertex (VertexModel vertex, SerializationWriter writer)
        {
            throw new NotImplementedException ();
        }
  
        /// <summary>
        /// Writes the edge.
        /// </summary>
        /// <param name='edge'>
        /// Edge.
        /// </param>
        /// <param name='writer'>
        /// Writer.
        /// </param>
        public static void WriteEdge (EdgeModel edge, SerializationWriter writer)
        {
            throw new NotImplementedException ();
        }
        
        #endregion
    }
}
