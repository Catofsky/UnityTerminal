using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;

public class SaveData : MonoBehaviour {

	public static void save(string data, string path, string fileName){
		if (!Directory.Exists (Application.dataPath + "/" + path)) {
			Directory.CreateDirectory (Application.dataPath + "/" + path);
		}
		FileStream fs = new FileStream (Application.dataPath + "/" + path + "/" + fileName, FileMode.Create);
		byte[] bts = Encoding.UTF8.GetBytes(data);
		fs.Write (bts, 0, bts.Length);
		fs.Close();
	}

	public static string load(string path){
		if (File.Exists (Application.dataPath + "/" + path)) {
			FileStream fs = new FileStream (Application.dataPath + "/" + path, FileMode.Open, FileAccess.Read);
			byte[] bts = new byte[fs.Length];
			int num = (int)fs.Length;
			int numb = 0;
			while (num > 0) {
				int i = fs.Read (bts, numb, num);
				if (i == 0)
					break;
				num -= i;
				numb += i;
			}
			return Encoding.UTF8.GetString (bts);
		} else {
			return "";
		}
	}

}
