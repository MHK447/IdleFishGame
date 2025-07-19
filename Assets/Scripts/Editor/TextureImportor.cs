using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextureImportor : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        // 가져오는 TextureImporter 객체
        TextureImporter importer = (TextureImporter)assetImporter;

        if (importer != null)
        {
            // 텍스처를 Sprite로 설정
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;

            // TextureImporterSettings를 수정
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            settings.alphaSource = TextureImporterAlphaSource.FromInput; // 알파 채널 포함 설정
            settings.sRGBTexture = true; // sRGB Texture 설정

            importer.SetTextureSettings(settings);

            // Android 플랫폼 설정
            SetPlatformTextureSettings(importer, "Android", TextureImporterFormat.ASTC_8x8);

            // iOS 플랫폼 설정
            SetPlatformTextureSettings(importer, "iPhone", TextureImporterFormat.ASTC_8x8);
        }
    }

    void SetPlatformTextureSettings(TextureImporter importer, string platformName, TextureImporterFormat format)
    {
        TextureImporterPlatformSettings platformSettings = new TextureImporterPlatformSettings
        {
            overridden = true,
            name = platformName,
            maxTextureSize = 2048, // 최대 텍스처 크기
            format = format,       // 포맷 설정 (ASTC 8x8)
            compressionQuality = 50 // 압축 품질 (0 ~ 100)
        };

        importer.SetPlatformTextureSettings(platformSettings);
    }
}
