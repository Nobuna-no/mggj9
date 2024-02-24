using UnityEngine;
using UnityEditor;
 
class TextureImporterUnityPattern : AssetPostprocessor {
    void OnPreprocessTexture () {
		if(assetPath.Contains("_MoveAgentMap")) {
       	 	TextureImporter textureImporter = assetImporter as TextureImporter;
        	//textureImporter.compressionQuality = (int)TextureCompressionQuality.Best;
        	//textureImporter.textureFormat = TextureImporterFormat.RGB24;
       		textureImporter.isReadable = true;
		}
   }
}