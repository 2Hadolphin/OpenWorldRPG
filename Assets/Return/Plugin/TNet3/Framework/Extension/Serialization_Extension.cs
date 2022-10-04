#define REFLECTION_SUPPORT

#define SERIALIZATION_WITHOUT_INTERFACE

// This can save some memory since the DataNodes' children list will not be initialized until AddChild() is called
#define DATANODE_CHILDREN_CAN_BE_NULL

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



namespace TNet
{

	public static partial class Serialization   // custom extension
	{
		[System.NonSerialized] static DataNode mTemp;

		//public static DataNode SetChildNode(this DataNode parent,string name, DataNode value)
		//{
		//	DataNode node = parent.GetChild(name);

		//	if (node == null) 
		//		node = parent.AddChild();

		//	node.Merge(value);
		//	node.name = name;
		//	return node;
		//}

		/// <summary>
		/// Load components from data node.
		/// </summary>
		static public Component[] InstanceIfNull(this DataNode data, Func<GameObject> replace, bool setActive = true)
		{
			var queue = new Queue<Component>();
			GameObject child = null;

			// load from AssetBundle
			var assetBytes = data.GetChild<byte[]>("assetBundle");
			
			if (assetBytes != null)
			{
				var ab = UnityTools.LoadAssetBundle(assetBytes);

				if (ab != null)
				{
					var all = ab.LoadAllAssets<GameObject>();
					var go = (all != null && all.Length > 0) ?
						all[0] : 
						null;

					if (go != null)
					{
						child = GameObject.Instantiate(go) as GameObject;
						child.name = data.name;
					}
				}
			}
			else
			{
				// load component

				// load prefab

				var path = data.GetChild<string>("prefab");

				if (!string.IsNullOrEmpty(path))
				{
					// load from resources or **addressable
					var prefab = UnityTools.LoadPrefab(path);

					if (prefab != null)
					{
						child = GameObject.Instantiate(prefab) as GameObject;
						child.name = data.name;
					}
					else if (replace != null)
					{
						var target = replace();
						target.name = data.name;
						child = target;
					}
					else
						child = new GameObject(data.name);
				}
				else if (replace != null)
				{
					var target = replace();
					target.name = data.name;
					child = target;
				}
				else 
					child = new GameObject(data.name);

				// In order to mimic Unity's prefabs, serialization should be performed on a disabled game object's hierarchy
				if (child.activeSelf) 
					child.SetActive(false);

				child.Deserialize(data,out var targets, true);
                foreach (var obj in targets)
                {
					if (obj is Component c)
						queue.Enqueue(c);

				}
			}

			if (child != null && setActive != child.activeSelf) 
				child.SetActive(setActive);

			//return child;

			return queue.ToArray();
		}

		public static object CastClass(this DataNode node,Type type=null)
        {
			if(type==null)
            {
				if (node.type == typeof(string))
					type = Serialization.NameToType((string)node.value);
				else if (node.type == typeof(Type))
					type = (Type)node.value;
				else
					Debug.LogError("Missing cast type");
            }
			return type == null ? null : type.Create();
			//return type == null? null: Activator.CreateInstance(type);
		}

		static void GetFields(object obj, ref Cache c)
		{
			Type type = obj.GetType();
			var fields = type.GetSerializableFields();

			for (int i = 0; i < fields.size; ++i)
			{
				var f = fields.buffer[i];
				var val = f.GetValue(obj);

				c.fieldNames.Add(f.Name);
				c.fieldValues.Add(val);
			}

			if (fields.size == 0 || type.IsDefined(typeof(SerializeProperties), true))
			{
				// We don't have fields to serialize, but we may have properties
				var props = type.GetSerializableProperties();

				if (props.size > 0)
				{
					for (int i = 0; i < props.size; ++i)
					{
						try
						{
							var prop = props.buffer[i];
							var val = prop.GetValue(obj, null);

							if (val != null)
							{
								c.fieldNames.Add(prop.Name);
								c.fieldValues.Add(val);
							}
						}
#if UNITY_EDITOR
						catch (Exception ex)
						{
							Debug.LogWarning(obj.GetType() + "." + props.buffer[i].Name + ": " + ex.Message + "\n" + ex.StackTrace, obj as UnityEngine.Object);
						}
#else
						catch (Exception) { }
#endif
					}
				}
			}
		}

		static public void CastFields(this DataNode node, object obj)
        {
			if(obj==null)
            {
				Debug.LogError(obj + " should not be null.");
				return;
            }				

            var c = GetCache();
            GetFields(obj, ref c);

            for (int i = 0, imax = c.fieldNames.size; i < imax; ++i)
            {
				var prop = obj.GetFieldOrProperty(c.fieldNames.buffer[i]);

				if(prop==null)
                {
					Debug.LogError("Missing cast " + c.fieldNames.buffer[i]);
					continue;
                }

				var target = node.FindChild(c.fieldNames.buffer[i]);
                if (target == null)
                {
					Debug.LogError("Missing cast data " + c.fieldNames.buffer[i]);
					continue;
				}

				prop.SetValue(obj, target.value);
            }

            RecycleCache(ref c);
        }

		/// <summary>
		/// Reflection data and save to dataNode.
		/// </summary>
		static public DataNode ReflectionSerialize(this DataNode node,object obj,string name)
        {
			// Try custom serialization first
			if (obj.Invoke("ReflectionSerialize", node))
				return null; // ???
	
			var cacheType= obj.GetType().GetCache();
			var typeName = Serialization.TypeToName(cacheType.type);

			//Debug.Log(cacheType.type.FullName +" : serialize type : " + typeName);

			var typeNode = node.AddChild("Type", typeName);

			// For MonoBehaviours we want to serialize serializable fields
			var fields = cacheType.GetSerializableFields();

			for (int f = 0; f < fields.size; ++f)
			{
				var field = fields.buffer[f];
				var val = field.GetValue(obj);

				if (val == null) 
					continue;

				//val = EncodeReference(go, val);
				//if (val == null) continue;

				node.AddChild(field.Name, val);
			}

			// Unity components don't have fields, so we should serialize properties instead.
			var props = cacheType.GetSerializableProperties();

			for (int f = 0; f < props.size; ++f)
			{
				var prop = props.buffer[f];

				// NOTE: Add any other fields that should not be serialized here
				if (prop.Name == "name" ||
					prop.Name == "tag" ||
					prop.Name == "hideFlags" ||
					prop.Name == "material" ||
					prop.Name == "materials")
                {
					Debug.LogError(prop.Name+" will not be serialized");
					continue;
				}

				object val = prop.GetValue(obj, null);

				if (val == null) 
					continue;

				//val = EncodeReference(go, val);
				//if (val == null) continue;

				node.AddChild(prop.Name, val);
			}

			return node;
		}

		/// <summary>
		/// Custom write node data
		/// </summary>
		static public void WriteData(this DataNode node,StreamWriter writer, int tab = 0)
		{
			// Only proceed if this node has some data associated with it
			if (tab == 0 && string.IsNullOrEmpty(node.name))
			{
				Write(writer, "Version", node.value != null ? node.value : Player.version);
			}
			else 
				Write(writer, string.IsNullOrEmpty(node.name) ? "DataNode" : node.name, node.value, tab);

			// Iterate through children
			if (node.children != null)
			{
				for (int i = 0; i < node.children.size; ++i)
				{
					var child = node.children.buffer[i];

					if (child.isSerializable)
					{
						writer.Write('\n');
						child.Write(writer, tab + 1);
					}
				}
			}

			if (tab == 0) writer.Flush();
		}

		static public void Write(StreamWriter writer, string name, object value, int tab = 0)
		{
			bool prefix = false;

			if (!string.IsNullOrEmpty(name))
			{
				prefix = true;
				writer.WriteTabs(tab);
				writer.Write(name);
			}
			else if (value != null)
			{
				writer.WriteTabs(tab);
			}

			if (value != null && !writer.WriteObject(value, prefix))
			{
				var type = value.GetType();

				// write node
				if (value is DataNode)
				{
					if (prefix) writer.Write(" = ");
					writer.Write("DataNode");
					writer.Write('\n');
					var node = (DataNode)value;
					node.Write(writer, tab + 1);
					return;
				}

#if !STANDALONE
				// write animationCurve
				if (value is AnimationCurve)
				{
					var ac = value as AnimationCurve;
					var kfs = ac.keys;
					type = typeof(Vector4[]);
					var imax = kfs.Length;
					var vs = new Vector4[imax];

					for (int i = 0; i < imax; ++i)
					{
						var kf = kfs[i];
						vs[i] = new Vector4(kf.time, kf.value, kf.inTangent, kf.outTangent);
					}
					value = vs;
				}
#endif
				// write clothSkinningCoefficient
				// Save cloth skinning coefficients as a Vector2 array
				if (value is ClothSkinningCoefficient[])
				{
					var cf = value as ClothSkinningCoefficient[];
					type = typeof(Vector2[]);
					var imax = cf.Length;
					var vs = new Vector2[imax];

					for (int i = 0; i < imax; ++i)
					{
						vs[i].x = cf[i].maxDistance;
						vs[i].y = cf[i].collisionSphereDistance;
					}
					value = vs;
				}

				// write TList
				if (value is TList)
				{
					var list = value as TList;

					if (prefix) writer.Write(" = ");
					writer.Write(Serialization.TypeToName(type));

					if (list.Count > 0)
					{
						for (int i = 0, imax = list.Count; i < imax; ++i)
						{
							writer.Write('\n');
							Write(writer, null, list.Get(i), tab + 1);
						}
					}
					return;
				}

				// write IList
				if (value is System.Collections.IList)
				{
					var list = value as System.Collections.IList;

					if (prefix) writer.Write(" = ");
					writer.Write(Serialization.TypeToName(type));

					if (list.Count > 0)
					{
						for (int i = 0, imax = list.Count; i < imax; ++i)
						{
							writer.Write('\n');
							Write(writer, null, list[i], tab + 1);
						}
					}
					return;
				}

				// write IDataNodeSerializeable
				// IDataNodeSerializable interface has serialization functions
				if (value is IDataNodeSerializable)
				{
					var ser = value as IDataNodeSerializable;
					var temp = mTemp;
					mTemp = null;
					if (temp == null) temp = new DataNode();
					ser.Serialize(temp);

					if (prefix) writer.Write(" = ");
					writer.Write(Serialization.TypeToName(type));

					if (temp.children != null)
					{
						for (int i = 0; i < temp.children.size; ++i)
						{
							var child = temp.children.buffer[i];
							writer.Write('\n');
							child.Write(writer, tab + 1);
						}
					}

					temp.Clear();
					mTemp = temp;
					return;
				}

#if REFLECTION_SUPPORT
#if SERIALIZATION_WITHOUT_INTERFACE
				// write reflectionSerialize
				// Try custom serialization first
				if (type.HasDataNodeSerialization())
				{
					var temp = mTemp;
					mTemp = null;
					if (temp == null) temp = new DataNode();

					if (value.Invoke("ReflectionSerialize", temp))
					{
						if (prefix) writer.Write(" = ");
						writer.Write(Serialization.TypeToName(type));

						if (temp.children != null)
						{
							for (int i = 0; i < temp.children.size; ++i)
							{
								var child = temp.children.buffer[i];
								writer.Write('\n');
								child.Write(writer, tab + 1);
							}
						}

						temp.Clear();
						mTemp = temp;
						return;
					}
				}
#endif

				// write reflection fields
				if (prefix)
					writer.Write(" = ");

				writer.Write(Serialization.TypeToName(type));
				var fields = type.GetSerializableFields();

				// We have fields to serialize
				for (int i = 0; i < fields.size; ++i)
				{
					var field = fields.buffer[i];
					var val = field.GetValue(value);

					if (val != null)
					{
#if !STANDALONE
						if (val is UnityEngine.Object)
						{
							var obj = val as UnityEngine.Object;
							if (!obj) 
								continue;
						}
#endif
						writer.Write('\n');
						Write(writer, field.Name, val, tab + 1);
					}
				}

				Debug.Log(nameof(fields.size) + fields.size);

				if (fields.size == 0 || type.IsDefined(typeof(SerializeProperties), true))
				{
					// We don't have fields to serialize, but we may have properties
					var props = type.GetSerializableProperties();

					if (props.size > 0)
					{
						for (int i = 0; i < props.size; ++i)
						{
							var prop = props.buffer[i];
							var val = prop.GetValue(value, null);

							if (val != null)
							{
#if !STANDALONE
								if (val is UnityEngine.Object)
								{
									var obj = val as UnityEngine.Object;
									if (!obj)
										continue;
								}
#endif
								writer.Write('\n');
								Write(writer, prop.Name, val, tab + 1);
							}
						}
					}
				}
#endif
			}
		}
    }



    public static partial class ComponentSerialization
	{
		static Queue<object> m_TempComps = new(10);

		/// <summary>
		/// Read data node and insitiate components.
		/// </summary>
		static public void Deserialize(this GameObject go, DataNode root,out IEnumerable<object> targets, bool includeChildren = true)
		{
			m_TempComps.Clear();

			//	|-Root
			//	|---Resources
			//	|---
			//	|-
			//	|-

			// Deserialize resources from resource or addressable
			{
				var resNode = root.GetChild("Resources");

				if (resNode != null && resNode.children != null)
				{
					for (int i = 0; i < resNode.children.size; ++i)
					{
						var child = resNode.children.buffer[i];

						if (child.name == "Texture")
							child.DeserializeTexture();

						else if (child.name == "Material")
							child.DeserializeMaterial();

						else if (child.name == "Mesh")
							child.DeserializeMesh();

						else if (child.name == "Clip")
							child.DeserializeClip();
					}
				}
			}

            {
				// load addressable

            }


			if (includeChildren)
			{
				// Deserialize the hierarchy, creating all game objects and components
				root.DeserializeHierarchy(go);
			}
            else
            {
				root.DeserializeComponents(go);
				root.DeserializeModules();
			}

			// Finish deserializing the components now that all other components are in place
			for (int i = 0; i < mSerList.size; ++i)
			{
				var ent = mSerList.buffer[i];

				try
				{
					ent.comp.Deserialize(ent.node);
				}
                catch (Exception e)
                {
					Debug.LogException(e);
					continue;
                }

				m_TempComps.Enqueue(ent.comp);
			}

			mSerList.Clear();

			targets = m_TempComps;
		}

		/// <summary>
		/// Custom deserialize hierarchy
		/// </summary>
		static void DeserializeHierarchy(this DataNode root, GameObject go)
		{
			var trans = go.transform;
			trans.localPosition = root.GetChild("position", trans.localPosition);
			trans.localEulerAngles = root.GetChild("rotation", trans.localEulerAngles);
			trans.localScale = root.GetChild("scale", trans.localScale);
			go.layer = root.GetChild("layer", go.layer);

			mLocalReferences[root.Get<int>()] = go;

			if (!root.GetChild<bool>("active", true))
				go.SetActive(false);

			var childNode = root.GetChild("Children");

			if (childNode != null && childNode.children != null && childNode.children.size > 0)
			{
				for (int i = 0; i < childNode.children.size; ++i)
				{
					var node = childNode.children.buffer[i];
					GameObject child = null;
					var prefab = UnityTools.Load<GameObject>(node.GetChild<string>("prefab"));

					if (prefab != null)
						child = GameObject.Instantiate(prefab) as GameObject;

					if (child == null) 
						child = new GameObject();

					child.name = node.name;

					var t = child.transform;
					t.parent = trans;
					t.localPosition = Vector3.zero;
					t.localRotation = Quaternion.identity;
					t.localScale = Vector3.one;

					AddReference(child, node.Get<int>());

					child.DeserializeHierarchy(node);
				}
			}

			go.DeserializeComponents(root);
		}

		/// <summary>
		/// Custom deserialize class
		/// </summary>
		/// <param name="root"></param>
		/// <param name="go"></param>
		static public void DeserializeComponents(this DataNode root,GameObject go)
		{
			var scriptNode = root.GetChild("Components");

			if (scriptNode == null || scriptNode.children == null) 
				return;

			for (int i = 0; i < scriptNode.children.size; ++i)
			{
				var node = scriptNode.children.buffer[i];
				var type = UnityTools.GetType(node.name);

				if (type != null && type.IsSubclassOf(typeof(Component)))
				{
					Component comp = null;
					if (type == typeof(ParticleSystemRenderer)) 
						comp = go.GetComponent(type);

					if (comp == null) 
						comp = go.AddComponent(type);
					if (comp == null) 
						continue; 

					// Can happen if two ParticleSystemRenderer get added to the same game object, for example
					ComponentSerialization.AddReference(comp, node.Get<int>());

                    var dc = new SerializationEntry
                    {
                        comp = comp,
                        node = node
                    };

                    mSerList.Add(dc);
				}
			}


		}


		static public void DeserializeModules(this DataNode root)
        {

        }
	}

}