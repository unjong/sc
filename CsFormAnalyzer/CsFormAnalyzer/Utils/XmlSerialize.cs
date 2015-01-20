using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CsFormAnalyzer.Utils
{
	public static class XmlSerialize
	{
		public static void ObjectToXml(string filename, object obj)
		{
			try
			{
				var SerializerObj = new XmlSerializer(obj.GetType());
				var WriteFileStream = new StreamWriter(filename);
				SerializerObj.Serialize(WriteFileStream, obj);
				WriteFileStream.Close();
			}
			catch(Exception ex)
			{
				Console.Write(ex);
			}
		}

		public static object XmlToObject(string filename, Type type)
		{
			try
			{
				var ReadFileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
				var SerializerObj = new XmlSerializer(type);
				var obj = SerializerObj.Deserialize(ReadFileStream);
				ReadFileStream.Close();

				return obj;
			}
			catch
			{
				return null;
			}
		}
	}
}
