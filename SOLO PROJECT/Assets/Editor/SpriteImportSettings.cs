using UnityEditor;
using UnityEngine;

// 이 스크립트는 Assets/Editor 폴더에 넣으면 자동 실행됩니다
public class SpriteImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        TextureImporter importer = (TextureImporter)assetImporter;
        string path = assetPath;

        // 배경 타일 설정
        if (path.Contains("Sprites/Background"))
        {
            importer.textureType         = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode          = FilterMode.Point;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled       = false;
            Debug.Log($"[SpriteImportSettings] 배경 타일 설정 적용: {path}");
        }

        // 플레이어 캐릭터 설정
        if (path.Contains("Sprites/Characters/Player"))
        {
            importer.textureType         = TextureImporterType.Sprite;
            importer.spriteImportMode    = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = 64;
            importer.filterMode          = FilterMode.Point;
            importer.textureCompression  = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled       = false;

            // 피벗 하단 중앙으로 설정
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spritePivot        = new Vector2(0.5f, 0f);
            settings.spriteAlignment    = (int)SpriteAlignment.BottomCenter;
            importer.SetTextureSettings(settings);

            Debug.Log($"[SpriteImportSettings] 플레이어 설정 적용: {path}");
        }
    }
}
