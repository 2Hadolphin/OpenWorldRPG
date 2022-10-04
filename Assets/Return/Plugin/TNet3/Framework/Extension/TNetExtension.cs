using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TNet;
using System;
using System.Text;

namespace Return
{
    public static class TNetExtension
    {
        public static IEnumerable<DataNode> GetEnumerable(this DataNode node)
        {
            if (node.IsNull())
                yield break;

            yield return node;

            if (node.children.IsNull() || node.children.Count == 0)
                yield break;

            var childs= node.children.GetEnumerator();

            while(childs.MoveNext())
            {
                foreach (var child in childs.Current.GetEnumerable())
                    yield return child;
            }

            yield break;
        }

        public static DataNode Read(byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            var reader = new StreamReader(stream);
            var node = DataNode.Read(reader);
            reader.Close();

            return node;
        }

        public static byte[] WriteBuffer(this DataNode node,out string str)
        {
            bool retVal = false;
            //byte[] array = new byte[1024];

            

            var stream = new MemoryStream();
            
            var writer = new StreamWriter(stream);
            node.Write(writer, 0);
            //retVal = Tools.WriteFile(path, stream, inMyDocuments, allowConfigAccess);

            
            //Debug.Log("Aarray length : "+stream.ToArray().Length);
            //Debug.Log("Buffer length : " + stream.GetBuffer().Length);

            var array= stream.ToArray();
            //if (stream.TryGetBuffer(out var buffer))
            //    array = buffer.ToArray();

            str  = Encoding.ASCII.GetString(array);
            //Debug.Log(str);
            var  remapArray = Encoding.ASCII.GetBytes(str);

            //Debug.Log(remapArray.Length == array.Length);

            //var fs = new FileStream(path, FileMode.Create);
            //stream.Seek(0, SeekOrigin.Begin);
            //stream.WriteTo();
            //fs.Flush();
            //fs.Close();

            writer.Close();
            stream.Close();

            return array;
        }
    }
}