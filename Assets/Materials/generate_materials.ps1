$matDir = "Assets\MUUKIA\Materials"

function Create-URPMaterial($fileName, $texGuid, [float]$smoothness = 0.1, [float]$metallic = 0.0, $emissionColor = "0,0,0", [float]$emissionIntensity = 0.0) {
    $hasEmission = $emissionIntensity -gt 0.0
    $emissionFlag = if ($hasEmission) { 1 } else { 0 }
    $emissionR, $emissionG, $emissionB = $emissionColor.Split(',')

    $yaml = @"
%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: $($fileName.Replace(".mat",""))
  m_Shader: {fileID: 4800000, guid: 933532a4fcc9baf4fa0491de14d08ed7, type: 3}
  m_Parent: {fileID: 0}
  m_ModifiedSerializedProperties: 0
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 0
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap:
    RenderType: Opaque
  disabledShaderPasses:
  - MOTIONVECTORS
  m_LockedProperties: 
  m_SavedProperties:
    serializedVersion: 3
    m_TexEnvs:
    - _BaseMap:
        m_Texture: {fileID: 2800000, guid: $texGuid, type: 3}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    - _MainTex:
        m_Texture: {fileID: 2800000, guid: $texGuid, type: 3}
        m_Scale: {x: 1, y: 1}
        m_Offset: {x: 0, y: 0}
    m_Ints: []
    m_Floats:
    - _Smoothness: $smoothness
    - _Metallic: $metallic
    - _EnvironmentReflections: 0
    - _SpecularHighlights: 0
    m_Colors:
    - _BaseColor: {r: 1, g: 1, b: 1, a: 1}
    - _Color: {r: 1, g: 1, b: 1, a: 1}
    - _EmissionColor: {r: $emissionR, g: $emissionG, b: $emissionB, a: 1}
"@
    [System.IO.File]::WriteAllText("$matDir\$fileName", $yaml)
}

Create-URPMaterial "M_Gustavo.mat" "295f55f6f8d2ca04c9f6a5e6b41899be" 0.05 0.0
Create-URPMaterial "M_Mother.mat" "d7c6e76220e904a4b9c7085b59374c43" 0.05 0.0
Create-URPMaterial "M_Neighbor.mat" "9952681ac98c2e44c82ce9a21a41e188" 0.05 0.0
Create-URPMaterial "M_Muukia.mat" "c3b2973340f8b3d4282c3c03dd168384" 0.0 0.0 "1.5,0.05,0.05" 2.0
Create-URPMaterial "M_Environment.mat" "e5fb1fc95b8e1c64395daaadf2caec50" 0.05 0.0
Create-URPMaterial "M_Props.mat" "39cbc9ac8684f314180d810ea23c9bfa" 0.1 0.0

Write-Output "Materials created successfully!"
