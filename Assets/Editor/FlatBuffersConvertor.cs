using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor;
using Debug = UnityEngine.Debug;

public class FlatBuffersConvertor
{
    private static readonly string DATA_CLASS_FOLDER = "Assets/Scripts/Game/Data/UserDataClass";
    private static readonly string FBS_PATH = "Assets/FlatBuffers/UserData.fbs";

    private static string FBS_CONTENT = "";

    

    [MenuItem("BanpoFri/FlatBuffersConvertor _F12")]
    public static void ConvertFlatBuffers()
    {
        string scriptPath = Path.Combine(Application.dataPath, "FlatBuffers", "convert.sh");
        
        // MacOS에서 실행 권한 부여
        Process.Start("chmod", "+x " + scriptPath).WaitForExit();
        
        // 스크립트 실행
        Process process = new Process();
        process.StartInfo.FileName = scriptPath;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        
        process.Start();
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        
        if (!string.IsNullOrEmpty(error))
        {
            UnityEngine.Debug.LogError("FlatBuffers 변환 오류: " + error);
        }
        else
        {

            // UserData.fbs 파일 읽기
            string fbsPath = Path.Combine(Application.dataPath, "..", FBS_PATH);
            if (!File.Exists(fbsPath)){ 
                UnityEngine.Debug.LogError($"UserData.fbs 파일을 찾을 수 없습니다. 경로: {fbsPath}");
                return;
            }

            FBS_CONTENT = File.ReadAllText(fbsPath);

            // 각 위치에 필요한 주석 포인트 , 함수 셋팅
            InitAutoKeyPoint();

            // DataClass폴더 하위에 각 데이터의 cs파일 생성 및 Class 작성
            GenerateDataClasses();

            // 새로 고침
            AssetDatabase.Refresh();
            
            UnityEngine.Debug.Log("FlatBuffers 변환 및 데이터 클래스 생성 완료");
        }
    }

    private static void InitAutoKeyPoint(){

        // UserData.cs 파일에 SetSaveDatas 함수 추가
        if (!AddFindStringData("UserData" , "@자동 저장 데이터 함수들" , "")){
            string addFunc = "void SetSaveDatas(FlatBufferBuilder builder){\n" +
                "        /* 아래 @주석 위치를 찾아서 함수가 자동 추가됩니다 SaveFile 함수에서 SetSaveDatas를 호출해주세요 */\n" +
                "        // @자동 저장 데이터 함수들\n" +
                "    }    ";
            AddFindStringData("UserData" , "private void SaveFile()" , addFunc , false , "" , "    ");

        }

        // UserData.cs 파일의 SetSaveDatas함수 위에 Action 타입 변수 추가
        if (!AddFindStringData("UserData" , "@저장 함수 콜백" , "")){
            string addString = "// @저장 함수 콜백\n    Action cb_SaveAddDatas = null;";
            AddFindStringData("UserData" , "void SetSaveDatas" , addString , false , "" , "    ");
        }

        if (!AddFindStringData("UserData" , "@저장 함수 콜백 호출" , "    cb_SaveAddDatas?.Invoke();\n        cb_SaveAddDatas = null;")){
            AddFindStringData("UserData" , "BanpoFri.Data.UserData.StartUserData(builder)" , "    // @저장 함수 콜백 호출");
            AddFindStringData("UserData" , "@저장 함수 콜백 호출" , "    cb_SaveAddDatas?.Invoke();\n        cb_SaveAddDatas = null;");
        }

        // UserData.cs 파일에 @add userdata 부분 추가
        if (!AddFindStringData("UserData" , "@add userdata" , "")){
            AddFindStringData("UserData" , "BanpoFri.Data.UserData.StartUserData" , "// @add userdata\n", false , "" , "        ");
        }


        // UserData.cs 파일에 SetSaveDatas 함수 호출부분 추가
        if (!AddFindStringData("UserData" , "SetSaveDatas(builder);" , "")){
            AddFindStringData("UserData" , "// @add userdata" , "SetSaveDatas(builder);", false , "" , "        ");
        }


        // UserData_Client.cs 파일에 SetLoadDatas 함수 추가
        if (!AddFindStringData("UserData_Client" , "@자동 로드 데이터 함수들" , "")){
            string addFunc = "void SetLoadDatas(){\n" +
                "        /* 아래 @주석 위치를 찾아서 함수가 자동 추가됩니다 ConnectReadOnlyDatas 함수에서 SetLoadDatas를 호출해주세요 */\n" +
                "        // @자동 로드 데이터 함수들\n" +
                "    }    ";
            AddFindStringData("UserData_Client" , "void ConnectReadOnlyDatas()" , addFunc , false , "" , "    ");
        }
        if (!AddFindStringData("UserData_Client" , "@로드 함수 호출" , "")){
            AddFindStringData("UserData_Client" , "ChangeDataMode(" , "// @로드 함수 호출\n\n        // @변수 자동 데이터 추가\n\n" , false , "" , "        ");
            AddFindStringData("UserData_Client" , "@로드 함수 호출" , "SetLoadDatas();" , true , "        ");
        }


        // UserData_Client.cs 파일에 @변수 자동 등록 위치 주석 추가
        if (!AddFindStringData("UserData_Client" , "@변수 자동 등록 위치" , "")){
            string userDataPath = Path.Combine(Application.dataPath, "Scripts", "Game", "Data", $"UserData_Client.cs");
            if (File.Exists(userDataPath)){
                string content = File.ReadAllText(userDataPath);
                var match = Regex.Match(content, @"^\s*(public|private|protected|internal)?\s*\w+\s+\w+\s*\(", RegexOptions.Multiline);
                if (match.Success) {
                    int index = match.Index;
                    string newContent = content.Insert(index, "\n    // @변수 자동 등록 위치\n\n");
                    File.WriteAllText(userDataPath, newContent);
                }

            }
        }

    }


    private static void GenerateDataClasses()
    {
        string userDataContent = GetExtractTableInfo("UserData");
        
        // 필드 파싱
        Dictionary<string, FieldInfo> fields = ParseFields(userDataContent);
        UnityEngine.Debug.Log($"파싱된 필드 수: {fields.Count}");

        List<string> createClassList = new List<string>();


        // 각 필드마다 파일 생성
        foreach (var field in fields)
        {
            // IsCustom이 아닐 경우엔
            if (!field.Value.IsCustom)
            {
                // UserData_Client.cs 파일에 변수 추가
                AddVariableToUserDataSystem(field.Value);
            }
            else{
                // IsCustom 타입이면
                // 우선 UserDataClass 폴더 하위에 cs파일을 생성
                if (!createClassList.Contains(field.Value.Type)){
                    createClassList.Add(field.Value.Type);
                }
                CreateCustomDataClass(field.Value);
            }

        }


        

        foreach (var className in createClassList){
            // 생성되거나 수정된 클래스의 저장 , 로드 함수 검증
            string existingCode = File.ReadAllText(Path.Combine(DATA_CLASS_FOLDER, $"{className}.cs"));
            string filePath = Path.Combine(DATA_CLASS_FOLDER, $"{className}.cs");
            
            // fbs 파일에서 해당 테이블 찾기
            string tableContent = GetExtractTableInfo(className);
            
            // 필드 파싱
            Dictionary<string, FieldInfo> classFields = ParseFields(tableContent, false);
            bool isUpdate = UpdateUserDataClassSaveLoad(ref existingCode, className , classFields);

            if (isUpdate) File.WriteAllText(filePath, existingCode);

            // UserData.cs 파일의 SetSaveDatas 함수에 저장 함수들 추가
            string saveFunctionName = $"SaveData_{className}";
            string loadFunctionName = $"LoadData_{className}";
            AddSaveLoadDataFunc("UserData" , saveFunctionName , "@자동 저장 데이터 함수들" , true);
            AddSaveLoadDataFunc("UserData_Client" , loadFunctionName , "@자동 로드 데이터 함수들" , false);

        }

    }


    // UserDataClass 폴더 하위에 cs파일을 생성
    private static void CreateCustomDataClass(FieldInfo field)
    {
        // UserDataClass 폴더가 존재하지 않으면 생성
        if (!Directory.Exists(DATA_CLASS_FOLDER))
        {
            Directory.CreateDirectory(DATA_CLASS_FOLDER);
            UnityEngine.Debug.Log("UserDataClass 폴더 생성");
        }

        string className = field.Type;
        string filePath = Path.Combine(DATA_CLASS_FOLDER, $"{className}.cs");
        
        // fbs 파일에서 해당 테이블 찾기
        string tableContent = GetExtractTableInfo(className);
        
        // 필드 파싱
        Dictionary<string, FieldInfo> classFields = ParseFields(tableContent, false);

        if (!File.Exists(filePath)){
            // 필수 클래스 형태 생성
            CreateNewDataClassFile(filePath, className, classFields);
        }

        // 해당 클래스 파일 업데이트 (데이터 검증)
        UpdateDataClassFile(filePath, className, classFields , field);
    }

    private static void UpdateDataClassFile(string filePath, string className, Dictionary<string, FieldInfo> classFields , FieldInfo field){
        // 데이터 검증
        string existingCode = File.ReadAllText(filePath);

        // 클래스의 필수 변수들 검증
        bool isBaseUpdate = UpdateBaseClassFields(ref existingCode, className, classFields);
        bool isUserDataUpdate = UpdateUserDataClassFields(ref existingCode, className , field);

        if (isBaseUpdate || isUserDataUpdate)
        {
            File.WriteAllText(filePath, existingCode);
        }
    }
    

    // 유저 데이터 클래스 체크
    private static bool UpdateUserDataClassFields(ref string existingCode, string className, FieldInfo field)
    {
        string propertyName = ToUpperFirst(field.Name);
        
        // FindClassEndPositionByName 함수를 사용하여 UserDataSystem 클래스 범위 찾기
        Match systemClassMatch = Regex.Match(existingCode, $@"public\s+partial\s+class\s+UserDataSystem[^{{]*{{");
        if (!systemClassMatch.Success)
        {
            UnityEngine.Debug.LogError($"UserDataSystem 클래스 정의를 찾을 수 없습니다.");
            return false;
        }
        
        int systemClassStart = systemClassMatch.Index + systemClassMatch.Length;
        int systemClassEnd = FindClassEndPositionByName(existingCode, "UserDataSystem", true);
        
        if (systemClassEnd == -1)
        {
            UnityEngine.Debug.LogError("UserDataSystem 클래스의 중괄호 쌍을 찾을 수 없습니다.");
            return false;
        }
        
        string systemClassContent = existingCode.Substring(systemClassStart, systemClassEnd - systemClassStart);
        
        // 필드가 이미 존재하는지 확인
        if (Regex.IsMatch(systemClassContent, $@"\b{Regex.Escape(propertyName)}\b"))
        {
            UnityEngine.Debug.Log($"기존 필드 발견: {propertyName}");
            return false;
        }
        
        // 새 필드 추가
        string classType = GetClassType(field, className);
        StringBuilder systemNewField = new StringBuilder();
        systemNewField.AppendLine();
        systemNewField.AppendLine($"    public {classType} {propertyName} {{ get; private set; }} = new {classType}();");
        
        existingCode = existingCode.Insert(systemClassStart, systemNewField.ToString());
        UnityEngine.Debug.Log($"{className}.cs 파일의 UserDataSystem 클래스에 {propertyName} 필드가 추가되었습니다.");
        return true;
    }



    // 베이스 클래스 변수 체크
    private static bool UpdateBaseClassFields(ref string existingCode, string className, Dictionary<string, FieldInfo> classFields)
    {
        // 기존 필드 추출
        HashSet<string> existingFields = ExtractExistingFields(existingCode);
        
        // 추가해야 할 필드 확인
        List<FieldInfo> fieldsToAdd = new List<FieldInfo>();
        // 타입이 변경된 필드 확인을 위한 리스트 추가
        List<FieldInfo> fieldsToUpdate = new List<FieldInfo>();
        Dictionary<string, bool> fieldCheckMap = new Dictionary<string, bool>();

        
        
        // 중첩된 타입을 저장할 리스트
        List<string> nestedTypesToCreate = new List<string>();
        
        foreach (var fieldItem in classFields)
        {
            var fieldInfo = fieldItem.Value;
            string propertyName = ToUpperFirst(fieldInfo.Name);
            
            // 필드 이름이 이미 처리되었는지 확인
            if (fieldCheckMap.ContainsKey(propertyName.ToLower()))
            {
                UnityEngine.Debug.LogWarning($"중복 필드 발견: {propertyName} - 무시됨");
                continue;
            }
            
            fieldCheckMap[propertyName.ToLower()] = true;

            // 커스텀 타입이고 중첩된 타입인 경우 목록에 추가
            if (fieldInfo.IsCustom)
            {
                string customClassName = fieldInfo.Type;
                // 해당 타입의 cs 파일이 존재하는지 확인
                string customFilePath = Path.Combine(DATA_CLASS_FOLDER, $"{customClassName}.cs");
                if (!File.Exists(customFilePath))
                {
                    // 파일이 존재하지 않으면 생성 목록에 추가
                    if (!nestedTypesToCreate.Contains(customClassName))
                    {
                        nestedTypesToCreate.Add(customClassName);
                        UnityEngine.Debug.Log($"중첩된 타입 발견: {customClassName} - 생성 목록에 추가됨");
                    }
                }
                else
                {
                    // 파일이 존재하는 경우 필드 업데이트 확인
                    string tableContent = GetExtractTableInfo(customClassName);
                    if (!string.IsNullOrEmpty(tableContent))
                    {
                        Dictionary<string, FieldInfo> nestedClassFields = ParseFields(tableContent, false);
                        string nestedExistingCode = File.ReadAllText(customFilePath);
                        bool isNestedUpdated = UpdateBaseClassFields(ref nestedExistingCode, customClassName, nestedClassFields);
                        
                        if (isNestedUpdated)
                        {
                            File.WriteAllText(customFilePath, nestedExistingCode);
                            UnityEngine.Debug.Log($"중첩된 타입 클래스 필드 업데이트됨: {customClassName}.cs");
                        }
                    }
                }
            }

            if (!existingFields.Contains(propertyName.ToLower()))
            {
                fieldsToAdd.Add(fieldInfo);
            }
            else
            {
                // 필드가 이미 존재하는 경우 타입 변경 여부 확인
                string strCheckTypePattern = $@"public\s+{GetCSharpType(fieldInfo)}\s+{propertyName}\s*{{";

                if (!Regex.IsMatch(existingCode, strCheckTypePattern))
                {
                    fieldsToUpdate.Add(fieldInfo);
                    UnityEngine.Debug.Log($"변경된 필드 발견: {propertyName} - ReactiveCollection으로 변경됨");
                }
            }
        }
        
        // 중첩된 타입의 클래스 생성
        foreach (var nestedType in nestedTypesToCreate)
        {
            // 해당 테이블의 정보 가져오기
            string tableContent = GetExtractTableInfo(nestedType);
            if (!string.IsNullOrEmpty(tableContent))
            {
                // 필드 파싱
                Dictionary<string, FieldInfo> nestedClassFields = ParseFields(tableContent, false);
                
                // 클래스 파일 생성
                string filePath = Path.Combine(DATA_CLASS_FOLDER, $"{nestedType}.cs");
                if (!File.Exists(filePath))
                {
                    // 새 클래스 파일 생성
                    CreateNewDataClassFile(filePath, nestedType, nestedClassFields);
                    UnityEngine.Debug.Log($"중첩된 타입을 위한 새 클래스 파일 생성됨: {nestedType}.cs");
                    
                    // 각 중첩된 타입에 대해서도 다시 UpdateBaseClassFields 호출
                    string newExistingCode = File.ReadAllText(filePath);
                    UpdateBaseClassFields(ref newExistingCode, nestedType, nestedClassFields);
                    File.WriteAllText(filePath, newExistingCode);
                }
                else
                {
                    // 기존 파일 업데이트 - 중첩된 필드들도 적절히 처리
                    string nestedExistingCode = File.ReadAllText(filePath);
                    bool isNestedUpdated = UpdateBaseClassFields(ref nestedExistingCode, nestedType, nestedClassFields);
                    
                    if (isNestedUpdated)
                    {
                        File.WriteAllText(filePath, nestedExistingCode);
                        UnityEngine.Debug.Log($"중첩된 타입 클래스 업데이트됨: {nestedType}.cs");
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"중첩된 타입 {nestedType}에 대한 테이블 정보를 찾을 수 없습니다.");
            }

        }
        
        bool isUpdated = false;
        
        if (fieldsToAdd.Count > 0)
        {
            // FindClassEndPositionByName 함수를 사용하여 클래스 정의 찾기
            Match classMatch = Regex.Match(existingCode, $@"public\s+class\s+{className}[^{{]*{{");
            if (!classMatch.Success)
            {
                UnityEngine.Debug.LogError($"{className} 클래스 정의를 찾을 수 없습니다.");
                return false;
            }
            
            // 필드 추가
            int insertPos = classMatch.Index + classMatch.Length;
            StringBuilder newFields = new StringBuilder();
            
            foreach (var fieldInfo in fieldsToAdd)
            {
                newFields.AppendLine();
                AddFieldToClassCode(newFields, fieldInfo);
            }

            existingCode = existingCode.Insert(insertPos, newFields.ToString());
            UnityEngine.Debug.Log($"{className}.cs 파일에 {fieldsToAdd.Count}개의 필드가 추가되었습니다.");
            isUpdated = true;
        }
        
        // 타입이 변경된 필드 업데이트
        if (fieldsToUpdate.Count > 0)
        {
            foreach (var fieldInfo in fieldsToUpdate)
            {
                string propertyName = ToUpperFirst(fieldInfo.Name);
                
                // 기존 필드 정의 패턴 - 다양한 형태를 커버
                string oldFieldPattern = $@"public\s+(?:[a-zA-Z0-9<>_]+)\s+{propertyName}\s*{{\s*get;\s*set;\s*}}\s*=\s*[^;]+;";
                
                // 새 필드 코드 생성
                StringBuilder newFieldBuilder = new StringBuilder();
                AddFieldToClassCode(newFieldBuilder, fieldInfo);
                string newFieldCode = newFieldBuilder.ToString().Trim();
                
                // 기존 필드 찾아 교체
                existingCode = Regex.Replace(existingCode, oldFieldPattern, newFieldCode);
                
                UnityEngine.Debug.Log($"{className}.cs 파일에서 필드 {propertyName}의 타입이 업데이트되었습니다.");
            }
            
            isUpdated = true;
        }
        
        return isUpdated;
    }

    private static bool UpdateUserDataClassSaveLoad(ref string existingCode, string className, Dictionary<string, FieldInfo> classFields){
        
        // Save 검증
        // SaveData_{ClassName} 함수 존재 여부 체크 후 존재하면 함수 내용까지 전부 지우고 새로 생성
        string saveFunctionPattern = $@"private\s+void\s+SaveData_{className}\s*\([^\)]*\)\s*";
        string loadFunctionPattern = $@"private\s+void\s+LoadData_{className}\s*\([^\)]*\)\s*";

        
        // Save,Load 함수 삭제
        existingCode = RemoveFunctionAndContents(existingCode, saveFunctionPattern);
        existingCode = RemoveFunctionAndContents(existingCode, loadFunctionPattern);


        // 새로운 SaveData_{ClassName} 함수 생성
        CreateNewSaveFunction(ref existingCode, className, classFields);
        
        // 새로운 LoadData_{ClassName} 함수 생성
        CreateNewLoadFunction(ref existingCode, className, classFields);


        return true;
    }

    private static void CreateNewSaveFunction(ref string existingCode, string className, Dictionary<string, FieldInfo> classFields){

        // UserDataClass 클래스의 끝부분에 새로운 SaveData_{ClassName} 함수 생성
        int classEnd = FindClassEndPositionByName(existingCode, "UserDataSystem", true);
        if (classEnd == -1)
        {
            UnityEngine.Debug.LogError("UserDataSystem 클래스의 끝을 찾을 수 없습니다.");
            return;
        }

        // 함수 코드 생성
        StringBuilder funcCode = new StringBuilder();
        // 클래스 마지막에 이미 공백 줄이 있는지 확인
        bool hasEmptyLine = HasEmptyLineBeforeBrace(existingCode, classEnd);
        
        // 빈 줄이 없을 경우에만 추가
        if (!hasEmptyLine)
        {
            funcCode.AppendLine();
        }
        
        // 함수 정의 추가
        funcCode.AppendLine("    private void SaveData_" + className + "(FlatBufferBuilder builder)");
        funcCode.AppendLine("    {");
        funcCode.AppendLine("        // 선언된 변수들은 모두 저장되어야함");
        funcCode.AppendLine();
        
        // 유저데이터 시스템에서 선언된 변수들 찾기
        var properties = FindUserDataSystemProperties(existingCode, className);


        List<string> list_AddDataLine = new List<string>();
        
        foreach (var property in properties)
        {
            string varName = property.Name;
            bool isList = property.IsList;
            bool isDictionary = property.IsDictionary;
            
            // 저장 로직 생성
            if (isList || isDictionary)
            {

                // 딕셔너리 타입 저장 로직
                funcCode.AppendLine($"        // {varName} Array 저장");
                funcCode.AppendLine($"        Offset<BanpoFri.Data.{className}>[] {varName.ToLower()}_Array = null;");
                funcCode.AppendLine($"        VectorOffset {varName.ToLower()}_Vector = default;");
                funcCode.AppendLine();
                funcCode.AppendLine($"        if({varName}.Count > 0){{");
                funcCode.AppendLine($"            {varName.ToLower()}_Array = new Offset<BanpoFri.Data.{className}>[{varName}.Count];");
                funcCode.AppendLine($"            int index = 0;");
                funcCode.AppendLine($"            foreach(var pair in {varName}){{");

                if (isList){ 
                    funcCode.AppendLine($"                var item = pair;"); 
                }
                else{ 
                    funcCode.AppendLine($"                var item = pair.Value;"); 
                }
                
                // 각 아이템의 필드 처리
                GenerateItemSaveCode(funcCode, className, classFields, "                ", "item");
                
                funcCode.AppendLine($"                {varName.ToLower()}_Array[index++] = BanpoFri.Data.{className}.Create{className}(");
                funcCode.AppendLine($"                    builder,");
                
                // 파라미터 생성
                GenerateParameterList(funcCode, classFields, "                    ", "item");
                
                funcCode.AppendLine($"                );");
                funcCode.AppendLine($"            }}");
                funcCode.AppendLine($"            {varName.ToLower()}_Vector = BanpoFri.Data.UserData.Create{ConvertToFlatBufferFieldName(varName)}Vector(builder, {varName.ToLower()}_Array);");
                funcCode.AppendLine($"        }}");
                funcCode.AppendLine();

                // Add는 하단에 한번에 처리
                list_AddDataLine.Add($"        BanpoFri.Data.UserData.Add{ConvertToFlatBufferFieldName(varName)}(builder, {varName.ToLower()}_Vector);");

            }
            else
            {
                // 단일 객체 타입 저장 로직
                funcCode.AppendLine($"        // {varName} 단일 저장");
                // 객체의 필드 처리
                GenerateItemSaveCode(funcCode, className, classFields, "        ", varName);
                
                funcCode.AppendLine($"        // {varName} 최종 생성 및 추가");
                funcCode.AppendLine($"        var {varName.ToLower()}_Offset = BanpoFri.Data.{className}.Create{className}(");
                funcCode.AppendLine($"            builder,");
                // 파라미터 생성
                GenerateParameterList(funcCode, classFields, "            ", varName);
                funcCode.AppendLine($"        );");

                // Add는 하단에 한번에 처리
                list_AddDataLine.Add($"        BanpoFri.Data.UserData.Add{ConvertToFlatBufferFieldName(varName)}(builder, {varName.ToLower()}_Offset);");
            }
        }



        funcCode.AppendLine();
        funcCode.AppendLine();

        funcCode.AppendLine("        Action cbAddDatas = () => {");

        // Add 처리
        foreach (var addLine in list_AddDataLine){
            funcCode.AppendLine("    " + addLine);
        }

        funcCode.AppendLine("        };");
        funcCode.AppendLine("");
        funcCode.AppendLine("        cb_SaveAddDatas += cbAddDatas;");
        funcCode.AppendLine("");
        funcCode.AppendLine("    }");

        // 클래스의 끝 부분에 함수 추가 (닫는 중괄호 바로 앞)
        existingCode = existingCode.Insert(classEnd - 1, funcCode.ToString());
    }

    // 유저 데이터 시스템 클래스에서 선언된 변수들을 찾아 반환하는 함수
    private static List<PropertyInfo> FindUserDataSystemProperties(string code, string className)
    {
        List<PropertyInfo> properties = new List<PropertyInfo>();
        
        // UserDataSystem 클래스 범위 찾기
        Match systemClassMatch = Regex.Match(code, $@"public\s+partial\s+class\s+UserDataSystem[^{{]*{{");
        if (!systemClassMatch.Success)
        {
            UnityEngine.Debug.LogError($"UserDataSystem 클래스 정의를 찾을 수 없습니다.");
            return properties;
        }
        
        int systemClassStart = systemClassMatch.Index + systemClassMatch.Length;
        int systemClassEnd = FindClassEndPositionByName(code, "UserDataSystem", true);
        
        if (systemClassEnd == -1)
        {
            UnityEngine.Debug.LogError("UserDataSystem 클래스의 중괄호 쌍을 찾을 수 없습니다.");
            return properties;
        }
        
        string systemClassContent = code.Substring(systemClassStart, systemClassEnd - systemClassStart);
        
        // 유저데이터 시스템에서 선언된 변수들 찾기
        MatchCollection propertyMatches = Regex.Matches(systemClassContent, 
            $@"public\s+(?:(?:List<{className}>)|{className}|Dictionary<int,\s*{className}>)\s+(\w+)\s*{{\s*get;\s*private\s*set;\s*}}\s*=\s*new\s+(?:(?:List<{className}>)|{className}|Dictionary<int,\s*{className}>)\(\);");
        
        foreach (Match propertyMatch in propertyMatches)
        {
            if (propertyMatch.Success && propertyMatch.Groups.Count > 1)
            {
                string varName = propertyMatch.Groups[1].Value;
                bool isList = Regex.IsMatch(propertyMatch.Value, $@"List<{className}>");
                bool isDictionary = Regex.IsMatch(propertyMatch.Value, $@"Dictionary<int,\s*{className}>");
                
                properties.Add(new PropertyInfo 
                { 
                    Name = varName, 
                    IsList = isList, 
                    IsDictionary = isDictionary 
                });
            }
        }
        
        return properties;
    }
    
    // 프로퍼티 정보를 담는 클래스
    private class PropertyInfo
    {
        public string Name { get; set; }
        public bool IsList { get; set; }
        public bool IsDictionary { get; set; }
    }

    // 아이템 필드 저장 로직 생성 (커스텀 객체, 딕셔너리 등)
    private static void GenerateItemSaveCode(StringBuilder funcCode, string className, Dictionary<string, FieldInfo> classFields, string indent, string itemName)
    {
        foreach (var fieldPair in classFields)
        {
            FieldInfo fieldInfo = fieldPair.Value;
            string fieldName = ToUpperFirst(fieldInfo.Name);
            
            // 변수 이름 충돌 방지를 위한 고유 접두사 생성
            string varPrefix = itemName.Replace(".", "_").ToLower() + "_" + fieldName.ToLower();

            // 필드 타입에 따른 처리
            if (fieldInfo.IsCustom)
            {
                // 커스텀 타입 필드
                funcCode.AppendLine($"{indent}// {itemName}.{fieldName} 처리 GenerateItemSaveCode IsCustom");

                var tableInfo = GetExtractTableInfo(fieldInfo.Type);
                // 필드 파싱
                Dictionary<string, FieldInfo> tableFields = ParseFields(tableInfo, false);
                
                if (fieldInfo.IsList || fieldInfo.IsDictionary || fieldInfo.IsReactiveCollection)
                {
                    funcCode.AppendLine($"{indent}Offset<BanpoFri.Data.{fieldInfo.Type}>[] {varPrefix}_Array = null;");
                    funcCode.AppendLine($"{indent}VectorOffset {varPrefix}_Vector = default;");
                    funcCode.AppendLine();
                    funcCode.AppendLine($"{indent}if({itemName}.{fieldName}.Count > 0){{");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Array = new Offset<BanpoFri.Data.{fieldInfo.Type}>[{itemName}.{fieldName}.Count];");
                    funcCode.AppendLine($"{indent}    int {varPrefix}_idx = 0;");
                    funcCode.AppendLine($"{indent}    foreach(var {varPrefix}_pair in {itemName}.{fieldName}){{");

                    if (fieldInfo.IsList){
                        // 아래에서 사용되는 변수명을 모두 소문자로 통일
                        funcCode.AppendLine($"{indent}        var {varPrefix}_item = {itemName}.{fieldName}[{varPrefix}_idx];");
                    }
                    else{
                        // 아래에서 사용되는 변수명을 모두 소문자로 통일
                        funcCode.AppendLine($"{indent}        var {varPrefix}_item = {varPrefix}_pair.Value;");
                    }

                    // 각 내부 항목마다 리스트/배열 필드 처리 코드 생성
                    // 변수명 대소문자 일관성을 위해 item을 소문자로 통일
                    ProcessCustomTypeFields(funcCode, tableFields, $"{indent}        ", $"{varPrefix}_item", fieldInfo.Type);
                    
                    funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = BanpoFri.Data.{fieldInfo.Type}.Create{fieldInfo.Type}(");
                    funcCode.AppendLine($"{indent}            builder,");
                    
                    int tableFieldCount = 0;
                    foreach (var tableField in tableFields){
                        tableFieldCount++;
                        string comma = tableFieldCount < tableFields.Count ? "," : "";
                        
                        if (tableField.Value.IsList || tableField.Value.IsDictionary) {
                            // 리스트/딕셔너리 필드는 벡터로 전달
                            // 변수명 대소문자 일관성을 위해 item을 소문자로 통일
                            string listVarName = $"{varPrefix}_item_{tableField.Value.Name.ToLower()}_Vector";
                            funcCode.AppendLine($"{indent}            {listVarName}{comma}");
                        }
                        else if (tableField.Value.Type == "string"){
                            // 변수명 대소문자 일관성을 위해 item을 소문자로 통일
                            funcCode.AppendLine($"{indent}            builder.CreateString({varPrefix}_item.{ToUpperFirst(tableField.Value.Name)}){comma}");
                        }
                        else if (tableField.Value.IsCustom) {
                            // 내부 커스텀 타입 필드는 오프셋으로 전달
                            // 변수명 대소문자 일관성을 위해 item을 소문자로 통일
                            string customVarName = $"{varPrefix}_item_{tableField.Value.Name.ToLower()}_Offset";
                            funcCode.AppendLine($"{indent}            {customVarName}{comma}");
                        }
                        else if (tableField.Value.IsReactive) {
                            // ReactiveProperty 필드는 Value 속성 값을 사용
                            funcCode.AppendLine($"{indent}            {varPrefix}_item.{ToUpperFirst(tableField.Value.Name)}.Value{comma}");
                        }
                        else{
                            // 변수명 대소문자 일관성을 위해 item을 소문자로 통일
                            funcCode.AppendLine($"{indent}            {varPrefix}_item.{ToUpperFirst(tableField.Value.Name)}{comma}");
                        }
                    }

                    funcCode.AppendLine($"{indent}        );");
                    funcCode.AppendLine($"{indent}    }}");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Vector = BanpoFri.Data.{className}.Create{fieldName}Vector(builder, {varPrefix}_Array);");
                    funcCode.AppendLine($"{indent}}}");
                    funcCode.AppendLine();
                }
                else
                {
                    // 단일 커스텀 타입 필드
                    // 내부 리스트/배열 필드 처리 코드 생성
                    ProcessCustomTypeFields(funcCode, tableFields, indent, $"{itemName}.{fieldName}", fieldInfo.Type);
                    
                    funcCode.AppendLine($"{indent}Offset<BanpoFri.Data.{fieldInfo.Type}> {varPrefix}_Offset = BanpoFri.Data.{fieldInfo.Type}.Create{fieldInfo.Type}(");
                    funcCode.AppendLine($"{indent}    builder, ");
                    int tableFieldCount = 0;
                    foreach (var tableField in tableFields){
                        tableFieldCount++;
                        string comma = tableFieldCount < tableFields.Count ? "," : "";

                        if (tableField.Value.IsList || tableField.Value.IsDictionary) {
                            // 리스트/딕셔너리 필드는 벡터로 전달
                            string listVarName = $"{itemName.Replace(".", "_").ToLower()}_{fieldName.ToLower()}_{tableField.Value.Name.ToLower()}_Vector";
                            funcCode.AppendLine($"{indent}    {listVarName}{comma}");
                        }
                        else if (tableField.Value.Type == "string"){
                            funcCode.AppendLine($"{indent}    builder.CreateString({itemName}.{fieldName}.{ToUpperFirst(tableField.Value.Name)}){comma}");
                        }
                        else if (tableField.Value.IsCustom) {
                            // 내부 커스텀 타입 필드는 오프셋으로 전달
                            string customVarName = $"{itemName.Replace(".", "_").ToLower()}_{fieldName.ToLower()}_{tableField.Value.Name.ToLower()}_Offset";
                            funcCode.AppendLine($"{indent}    {customVarName}{comma}");
                        }
                        else if (tableField.Value.IsReactive) {
                            // ReactiveProperty 필드는 Value 속성 값을 사용
                            funcCode.AppendLine($"{indent}    {itemName}.{fieldName}.{ToUpperFirst(tableField.Value.Name)}.Value{comma}");
                        }
                        else{
                            funcCode.AppendLine($"{indent}    {itemName}.{fieldName}.{ToUpperFirst(tableField.Value.Name)}{comma}");
                        }
                    }
                    funcCode.AppendLine($"{indent});");
                    funcCode.AppendLine();
                }
            }
            else if (fieldInfo.IsList || fieldInfo.IsDictionary)
            {
                // 리스트 또는 딕셔너리 필드
                funcCode.AppendLine($"{indent}// {itemName}.{fieldName} 처리 GenerateItemSaveCode Array");
                
                // 기본 타입 리스트/딕셔너리 처리
                if (fieldInfo.Type.ToLower() == "string") 
                {
                    // 문자열 리스트 처리
                    funcCode.AppendLine($"{indent}StringOffset[] {varPrefix}_Array = null;");
                    funcCode.AppendLine($"{indent}VectorOffset {varPrefix}_Vector = default;");
                    funcCode.AppendLine();
                    funcCode.AppendLine($"{indent}if({itemName}.{fieldName}.Count > 0){{");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Array = new StringOffset[{itemName}.{fieldName}.Count];");
                    funcCode.AppendLine($"{indent}    int {varPrefix}_idx = 0;");
                    
                    if (fieldInfo.IsList) 
                    {
                        funcCode.AppendLine($"{indent}    foreach(string {varPrefix}_str in {itemName}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = builder.CreateString({varPrefix}_str);");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}    foreach(var {varPrefix}_pair in {itemName}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = builder.CreateString({varPrefix}_pair.Value);");
                    }
                    
                    funcCode.AppendLine($"{indent}    }}");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Vector = BanpoFri.Data.{className}.Create{fieldName}Vector(builder, {varPrefix}_Array);");
                    funcCode.AppendLine($"{indent}}}");
                }
                else
                {
                    // 다른 기본 타입 리스트 처리 (int, bool 등)
                    funcCode.AppendLine($"{indent}{fieldInfo.Type}[] {varPrefix}_Array = null;");
                    funcCode.AppendLine($"{indent}VectorOffset {varPrefix}_Vector = default;");
                    funcCode.AppendLine();
                    funcCode.AppendLine($"{indent}if({itemName}.{fieldName}.Count > 0){{");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Array = new {fieldInfo.Type}[{itemName}.{fieldName}.Count];");
                    funcCode.AppendLine($"{indent}    int {varPrefix}_idx = 0;");
                    
                    if (fieldInfo.IsList)
                    {
                        funcCode.AppendLine($"{indent}    foreach({fieldInfo.Type} {varPrefix}_val in {itemName}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = {varPrefix}_val;");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}    foreach(var {varPrefix}_pair in {itemName}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = {varPrefix}_pair.Value;");
                    }
                    
                    funcCode.AppendLine($"{indent}    }}");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Vector = BanpoFri.Data.{className}.Create{fieldName}Vector(builder, {varPrefix}_Array);");
                    funcCode.AppendLine($"{indent}}}");
                }
                
                funcCode.AppendLine();
            }
            // 다른 기본 타입들은 파라미터로 직접 전달하므로 여기서는 처리하지 않음
        }
    }
    
    // 커스텀 타입 내의 필드들을 처리하는 헬퍼 메서드
    private static void ProcessCustomTypeFields(StringBuilder funcCode, Dictionary<string, FieldInfo> tableFields, string indent, string itemPath, string typeName)
    {
        foreach (var tableField in tableFields)
        {
            if (tableField.Value.IsList || tableField.Value.IsDictionary)
            {
                string fieldName = ToUpperFirst(tableField.Value.Name);
                // 모든 변수명은 소문자로 통일
                string varPrefix = itemPath.Replace(".", "_").ToLower() + "_" + tableField.Value.Name.ToLower();
                
                if (tableField.Value.Type.ToLower() == "string")
                {
                    // 문자열 리스트 처리
                    funcCode.AppendLine($"{indent}// {itemPath}.{fieldName} 처리 GenerateItemSaveCode Array");
                    funcCode.AppendLine($"{indent}StringOffset[] {varPrefix}_Array = null;");
                    funcCode.AppendLine($"{indent}VectorOffset {varPrefix}_Vector = default;");
                    funcCode.AppendLine();
                    funcCode.AppendLine($"{indent}if({itemPath}.{fieldName}.Count > 0){{");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Array = new StringOffset[{itemPath}.{fieldName}.Count];");
                    funcCode.AppendLine($"{indent}    int {varPrefix}_idx = 0;");
                    
                    if (tableField.Value.IsList)
                    {
                        funcCode.AppendLine($"{indent}    foreach(string {varPrefix}_str in {itemPath}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = builder.CreateString({varPrefix}_str);");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}    foreach(var {varPrefix}_pair in {itemPath}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = builder.CreateString({varPrefix}_pair.Value);");
                    }
                    
                    funcCode.AppendLine($"{indent}    }}");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Vector = BanpoFri.Data.{typeName}.Create{fieldName}Vector(builder, {varPrefix}_Array);");
                    funcCode.AppendLine($"{indent}}}");
                    funcCode.AppendLine();
                }
                else if (tableField.Value.IsCustom)
                {
                    // 커스텀 타입 리스트 처리 추가
                    funcCode.AppendLine($"{indent}// {itemPath}.{fieldName} 처리 GenerateItemSaveCode IsCustom");
                    funcCode.AppendLine($"{indent}Offset<BanpoFri.Data.{tableField.Value.Type}>[] {varPrefix}_Array = null;");
                    funcCode.AppendLine($"{indent}VectorOffset {varPrefix}_Vector = default;");
                    funcCode.AppendLine();
                    funcCode.AppendLine($"{indent}if({itemPath}.{fieldName}.Count > 0){{");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Array = new Offset<BanpoFri.Data.{tableField.Value.Type}>[{itemPath}.{fieldName}.Count];");
                    funcCode.AppendLine($"{indent}    int {varPrefix}_idx = 0;");
                    
                    if (tableField.Value.IsList)
                    {
                        funcCode.AppendLine($"{indent}    foreach(var {varPrefix}_pair in {itemPath}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        var {varPrefix}_item = {itemPath}.{fieldName}[{varPrefix}_idx];");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}    foreach(var {varPrefix}_pair in {itemPath}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        var {varPrefix}_item = {varPrefix}_pair.Value;");
                    }
                    
                    // 내부 커스텀 타입의 테이블 정보 가져오기
                    var nestedTableInfo = GetExtractTableInfo(tableField.Value.Type);
                    Dictionary<string, FieldInfo> nestedFields = ParseFields(nestedTableInfo, false);
                    
                    // 내부 필드 처리
                    ProcessCustomTypeFields(funcCode, nestedFields, $"{indent}        ", $"{varPrefix}_item", tableField.Value.Type);
                    
                    // 오프셋 생성
                    funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = BanpoFri.Data.{tableField.Value.Type}.Create{tableField.Value.Type}(");
                    funcCode.AppendLine($"{indent}            builder,");
                    
                    int nestedFieldCount = 0;
                    foreach (var nestedField in nestedFields)
                    {
                        nestedFieldCount++;
                        string comma = nestedFieldCount < nestedFields.Count ? "," : "";
                        
                        if (nestedField.Value.IsList || nestedField.Value.IsDictionary)
                        {
                            string nestedVarName = $"{varPrefix}_item_{nestedField.Value.Name.ToLower()}_Vector";
                            funcCode.AppendLine($"{indent}            {nestedVarName}{comma}");
                        }
                        else if (nestedField.Value.Type == "string")
                        {
                            funcCode.AppendLine($"{indent}            builder.CreateString({varPrefix}_item.{ToUpperFirst(nestedField.Value.Name)}){comma}");
                        }
                        else if (nestedField.Value.IsCustom)
                        {
                            string nestedVarName = $"{varPrefix}_item_{nestedField.Value.Name.ToLower()}_Offset";
                            funcCode.AppendLine($"{indent}            {nestedVarName}{comma}");
                        }
                        else if (nestedField.Value.IsReactive)
                        {
                            // ReactiveProperty 필드는 Value 속성 값을 사용
                            funcCode.AppendLine($"{indent}            {varPrefix}_item.{ToUpperFirst(nestedField.Value.Name)}.Value{comma}");
                        }
                        else
                        {
                            funcCode.AppendLine($"{indent}            {varPrefix}_item.{ToUpperFirst(nestedField.Value.Name)}{comma}");
                        }
                    }
                    
                    funcCode.AppendLine($"{indent}        );");
                    funcCode.AppendLine($"{indent}    }}");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Vector = BanpoFri.Data.{typeName}.Create{fieldName}Vector(builder, {varPrefix}_Array);");
                    funcCode.AppendLine($"{indent}}}");
                    funcCode.AppendLine();
                }
                else
                {
                    // 다른 기본 타입 리스트 처리
                    funcCode.AppendLine($"{indent}// {itemPath}.{fieldName} 처리 GenerateItemSaveCode Array");
                    funcCode.AppendLine($"{indent}{tableField.Value.Type}[] {varPrefix}_Array = null;");
                    funcCode.AppendLine($"{indent}VectorOffset {varPrefix}_Vector = default;");
                    funcCode.AppendLine();
                    funcCode.AppendLine($"{indent}if({itemPath}.{fieldName}.Count > 0){{");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Array = new {tableField.Value.Type}[{itemPath}.{fieldName}.Count];");
                    funcCode.AppendLine($"{indent}    int {varPrefix}_idx = 0;");
                    
                    if (tableField.Value.IsList)
                    {
                        funcCode.AppendLine($"{indent}    foreach({tableField.Value.Type} {varPrefix}_val in {itemPath}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = {varPrefix}_val;");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}    foreach(var {varPrefix}_pair in {itemPath}.{fieldName}){{");
                        funcCode.AppendLine($"{indent}        {varPrefix}_Array[{varPrefix}_idx++] = {varPrefix}_pair.Value;");
                    }
                    
                    funcCode.AppendLine($"{indent}    }}");
                    funcCode.AppendLine($"{indent}    {varPrefix}_Vector = BanpoFri.Data.{typeName}.Create{fieldName}Vector(builder, {varPrefix}_Array);");
                    funcCode.AppendLine($"{indent}}}");
                    funcCode.AppendLine();
                }
            }
            else if (tableField.Value.IsCustom)
            {
                // 내부 커스텀 타입 필드 처리
                string fieldName = ToUpperFirst(tableField.Value.Name);
                // 모든 변수명은 소문자로 통일
                string varPrefix = itemPath.Replace(".", "_").ToLower() + "_" + tableField.Value.Name.ToLower();
                
                funcCode.AppendLine($"{indent}// {itemPath}.{fieldName} 처리 GenerateItemSaveCode IsCustom Nested");
                
                // 재귀적으로 내부 커스텀 타입 처리
                var nestedTableInfo = GetExtractTableInfo(tableField.Value.Type);
                Dictionary<string, FieldInfo> nestedFields = ParseFields(nestedTableInfo, false);
                
                ProcessCustomTypeFields(funcCode, nestedFields, indent, $"{itemPath}.{fieldName}", tableField.Value.Type);
                
                funcCode.AppendLine($"{indent}Offset<BanpoFri.Data.{tableField.Value.Type}> {varPrefix}_Offset = BanpoFri.Data.{tableField.Value.Type}.Create{tableField.Value.Type}(");
                funcCode.AppendLine($"{indent}    builder,");
                
                int nestedFieldCount = 0;
                foreach (var nestedField in nestedFields)
                {
                    nestedFieldCount++;
                    string comma = nestedFieldCount < nestedFields.Count ? "," : "";
                    
                    if (nestedField.Value.IsList || nestedField.Value.IsDictionary)
                    {
                        // 모든 변수명은 소문자로 통일
                        string nestedVarName = $"{itemPath.Replace(".", "_").ToLower()}_{fieldName.ToLower()}_{nestedField.Value.Name.ToLower()}_Vector";
                        funcCode.AppendLine($"{indent}    {nestedVarName}{comma}");
                    }
                    else if (nestedField.Value.Type == "string")
                    {
                        funcCode.AppendLine($"{indent}    builder.CreateString({itemPath}.{fieldName}.{ToUpperFirst(nestedField.Value.Name)}){comma}");
                    }
                    else if (nestedField.Value.IsCustom)
                    {
                        // 모든 변수명은 소문자로 통일
                        string nestedVarName = $"{itemPath.Replace(".", "_").ToLower()}_{fieldName.ToLower()}_{nestedField.Value.Name.ToLower()}_Offset";
                        funcCode.AppendLine($"{indent}    {nestedVarName}{comma}");
                    }
                    else if (nestedField.Value.IsReactive)
                    {
                        // ReactiveProperty 필드는 Value 속성 값을 사용
                        funcCode.AppendLine($"{indent}    {itemPath}.{fieldName}.{ToUpperFirst(nestedField.Value.Name)}.Value{comma}");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}    {itemPath}.{fieldName}.{ToUpperFirst(nestedField.Value.Name)}{comma}");
                    }
                }
                
                funcCode.AppendLine($"{indent});");
                funcCode.AppendLine();
            }
        }
    }
    
    // Create 함수의 파라미터 리스트 생성
    private static void GenerateParameterList(StringBuilder funcCode, Dictionary<string, FieldInfo> classFields, string indent, string itemName)
    {
        int count = 0;
        foreach (var fieldPair in classFields)
        {
            count++;
            FieldInfo fieldInfo = fieldPair.Value;
            string fieldName = ToUpperFirst(fieldInfo.Name);
            
            // 변수 이름 충돌 방지를 위한 고유 접두사 생성
            string varPrefix = itemName.Replace(".", "_").ToLower() + "_" + fieldName.ToLower();
            
            string comma = count < classFields.Count ? "," : "";
            
            if (fieldInfo.IsCustom)
            {
                if (fieldInfo.IsList || fieldInfo.IsDictionary)
                {
                    // 배열/딕셔너리 필드
                    funcCode.AppendLine($"{indent}{varPrefix}_Vector{comma}");
                }
                else
                {
                    // 단일 커스텀 타입 필드
                    funcCode.AppendLine($"{indent}{varPrefix}_Offset{comma}");
                }
            }
            else if (fieldInfo.IsList || fieldInfo.IsDictionary || fieldInfo.IsReactiveCollection)
            {
                // 리스트/딕셔너리/ReactiveCollection 필드
                funcCode.AppendLine($"{indent}{varPrefix}_Vector{comma}");
            }
            else
            {
                // 기본 타입 필드
                if (fieldInfo.Type == "string"){
                    funcCode.AppendLine($"{indent}builder.CreateString({itemName}.{fieldName}){comma}");
                }
                else if (fieldInfo.IsDateTime){
                    // DateTime 타입은 Ticks 값을 long으로 저장
                    funcCode.AppendLine($"{indent}{itemName}.{fieldName}.Ticks{comma}");
                }
                else if (fieldInfo.IsReactive){
                    // ReactiveProperty 타입은 Value 속성 값을 사용
                    funcCode.AppendLine($"{indent}{itemName}.{fieldName}.Value{comma}");
                }
                else{
                    funcCode.AppendLine($"{indent}{itemName}.{fieldName}{comma}");
                }
            }
        }
    }

    private static void CreateNewLoadFunction(ref string existingCode, string className, Dictionary<string, FieldInfo> classFields){

        // UserDataClass 클래스의 끝부분에 새로운 LoadData_{ClassName} 함수 생성
        int classEnd = FindClassEndPositionByName(existingCode, "UserDataSystem", true);
        if (classEnd == -1)
        {
            UnityEngine.Debug.LogError("UserDataSystem 클래스의 끝을 찾을 수 없습니다.");
            return;
        }

        // 함수 코드 생성
        StringBuilder funcCode = new StringBuilder();
        // 클래스 마지막에 이미 공백 줄이 있는지 확인
        bool hasEmptyLine = HasEmptyLineBeforeBrace(existingCode, classEnd);
        
        // 빈 줄이 없을 경우에만 추가
        if (!hasEmptyLine)
        {
            funcCode.AppendLine();
        }
        
        // 유저데이터 시스템에서 선언된 변수들 찾기
        var properties = FindUserDataSystemProperties(existingCode, className);
        
        // 함수 정의 추가
        funcCode.AppendLine("    private void LoadData_" + className + "()");
        funcCode.AppendLine("    {");
        funcCode.AppendLine("        // 로드 함수 내용");
        
        if (properties.Count > 0)
        {
            // 각 변수에 대한 로드 코드 생성
            foreach (var property in properties)
            {
                string varName = property.Name;
                bool isList = property.IsList;
                bool isDictionary = property.IsDictionary;
                bool isReactiveCollection = property.IsDictionary;
                string fbFieldName = ConvertToFlatBufferFieldName(varName);
                
                
                if (isList || isDictionary || isReactiveCollection)
                {
                    // 리스트/딕셔너리 로드 코드 생성
                    funcCode.AppendLine();
                    funcCode.AppendLine($"        // {varName} 로드");
                    funcCode.AppendLine($"        {varName}.Clear();");
                    funcCode.AppendLine($"        int {fbFieldName}_length = flatBufferUserData.{fbFieldName}Length;");
                    funcCode.AppendLine($"        for (int i = 0; i < {fbFieldName}_length; i++)");
                    funcCode.AppendLine($"        {{");
                    funcCode.AppendLine($"            var {fbFieldName}_item = flatBufferUserData.{fbFieldName}(i);");
                    funcCode.AppendLine($"            if ({fbFieldName}_item.HasValue)");
                    funcCode.AppendLine($"            {{");
                    funcCode.AppendLine($"                var {className.ToLower()} = new {className}");
                    funcCode.AppendLine($"                {{");
                    
                    // 각 필드에 대한 로드 코드 생성 (초기화 블록용 new 인수 추가)
                    GenerateFieldLoadCode(funcCode, classFields, "                    ", $"new {className}", $"{fbFieldName}_item.Value");
                    
                    funcCode.AppendLine($"                }};");
                    
                    // 중첩된 객체와 리스트 필드 처리 코드 생성
                    GenerateNestedObjectLoadCode(funcCode, classFields, "                ", $"{className.ToLower()}", $"{fbFieldName}_item.Value");
                    
                    // 리스트/딕셔너리에 저장하는 코드
                    if (isList)
                    {
                        funcCode.AppendLine($"                {varName}.Add({className.ToLower()});");
                    }
                    else // 딕셔너리
                    {
                        funcCode.AppendLine($"                {varName}[{className.ToLower()}.Idx] = {className.ToLower()};");
                    }
                    
                    funcCode.AppendLine($"            }}");
                    funcCode.AppendLine($"        }}");
                }
                else
                {
                    // 단일 객체 로드 코드 생성
                    funcCode.AppendLine();
                    funcCode.AppendLine($"        // {varName} 로드");
                    funcCode.AppendLine($"        var fb_{fbFieldName} = flatBufferUserData.{fbFieldName};");
                    funcCode.AppendLine($"        if (fb_{fbFieldName}.HasValue)");
                    funcCode.AppendLine($"        {{");
                    
                    // 기본 필드 로드 코드 생성
                    GenerateFieldLoadCode(funcCode, classFields, "            ", varName, $"fb_{fbFieldName}.Value");
                    
                    // 중첩된 객체와 리스트 필드 처리 코드 생성
                    GenerateNestedObjectLoadCode(funcCode, classFields, "            ", varName, $"fb_{fbFieldName}.Value");
                    
                    funcCode.AppendLine($"        }}");
                }
            }
        }
        
        funcCode.AppendLine("    }");

        // 클래스의 끝 부분에 함수 추가 (닫는 중괄호 바로 앞)
        existingCode = existingCode.Insert(classEnd - 1, funcCode.ToString());
    }
    
    // 기본 필드 로드 코드 생성
    private static void GenerateFieldLoadCode(StringBuilder funcCode, Dictionary<string, FieldInfo> classFields, string indent, string targetVar, string sourceVar)
    {
        int fieldCount = 0;
        bool isInitializerBlock = targetVar.Contains("new ");
        
        foreach (var field in classFields)
        {
            fieldCount++;
            string fieldName = ToUpperFirst(field.Value.Name);
            string comma = fieldCount < classFields.Count && isInitializerBlock ? "," : "";
            string terminator = isInitializerBlock ? "" : ";";
            
            // 커스텀 타입이 아니고 리스트/딕셔너리가 아닌 기본 필드만 처리
            if (!field.Value.IsCustom && !field.Value.IsList && !field.Value.IsDictionary)
            {
                // DateTime 타입인 경우 Ticks에서 변환
                if (field.Value.IsDateTime)
                {
                    if (isInitializerBlock)
                    {
                        funcCode.AppendLine($"{indent}{fieldName} = new DateTime({sourceVar}.{fieldName}){comma}{terminator}");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}{targetVar}.{fieldName} = new DateTime({sourceVar}.{fieldName}){terminator}");
                    }
                }
                else if (field.Value.IsReactive)
                {
                    // ReactiveProperty 타입은 Value 속성 값을 사용
                    if (isInitializerBlock)
                    {
                        funcCode.AppendLine($"{indent}{fieldName} = new ReactiveProperty<{GetBasicCSharpType(field.Value.Type)}>({sourceVar}.{fieldName}){comma}{terminator}");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}{targetVar}.{fieldName}.Value = {sourceVar}.{fieldName}{comma}{terminator}");
                    }
                }
                // 초기화 블록의 경우 속성명만 쓰고, 아닌 경우 객체.속성명으로 접근
                else if (isInitializerBlock)
                {
                    funcCode.AppendLine($"{indent}{fieldName} = {sourceVar}.{fieldName}{comma}{terminator}");
                }
                else
                {
                    funcCode.AppendLine($"{indent}{targetVar}.{fieldName} = {sourceVar}.{fieldName}{terminator}");
                }
            }
        }
    }
    
    // 중첩된 객체 구조 로드 코드 생성
    private static void GenerateNestedObjectLoadCode(StringBuilder funcCode, Dictionary<string, FieldInfo> classFields, string indent, string targetVar, string sourceVar)
    {
        foreach (var field in classFields)
        {
            string fieldName = ToUpperFirst(field.Value.Name);
            
            // 커스텀 타입인 필드 처리
            if (field.Value.IsCustom)
            {
                if (field.Value.IsList || field.Value.IsDictionary || field.Value.IsReactiveCollection)
                {
                    // 배열/딕셔너리/ReactiveCollection 필드 처리
                    funcCode.AppendLine();
                    funcCode.AppendLine($"{indent}// {fieldName} 로드");
                    funcCode.AppendLine($"{indent}{targetVar}.{fieldName}.Clear();");
                    funcCode.AppendLine($"{indent}int {fieldName.ToLower()}Length = {sourceVar}.{fieldName}Length;");
                    funcCode.AppendLine($"{indent}for (int j = 0; j < {fieldName.ToLower()}Length; j++)");
                    funcCode.AppendLine($"{indent}{{");
                    funcCode.AppendLine($"{indent}    var fb{fieldName}Item = {sourceVar}.{fieldName}(j);");
                    funcCode.AppendLine($"{indent}    if (fb{fieldName}Item.HasValue)");
                    funcCode.AppendLine($"{indent}    {{");
                    funcCode.AppendLine($"{indent}        var nested_item = new {field.Value.Type}");
                    funcCode.AppendLine($"{indent}        {{");
                    
                    // 테이블 정보 가져오기
                    var tableInfo = GetExtractTableInfo(field.Value.Type);
                    Dictionary<string, FieldInfo> tableFields = ParseFields(tableInfo, false);
                    
                    // 각 필드에 대한 로드 코드 생성 - 리스트가 아닌 기본 필드만 이곳에서 처리
                    int fieldIdx = 0;
                    foreach (var tableField in tableFields)
                    {
                        // 리스트 필드는 이 블록에서 처리하지 않음 (나중에 별도로 처리)
                        if (!tableField.Value.IsList && !tableField.Value.IsDictionary && !tableField.Value.IsCustom)
                        {
                            fieldIdx++;
                            string tableFieldName = ToUpperFirst(tableField.Value.Name);
                            string comma = fieldIdx < tableFields.Count ? "," : "";
                            
                            if (tableField.Value.IsReactive)
                            {
                                // ReactiveProperty는 새 인스턴스 생성
                                funcCode.AppendLine($"{indent}            {tableFieldName} = new ReactiveProperty<{GetBasicCSharpType(tableField.Value.Type)}>(fb{fieldName}Item.Value.{tableFieldName}){comma}");
                            }
                            else
                            {
                                funcCode.AppendLine($"{indent}            {tableFieldName} = fb{fieldName}Item.Value.{tableFieldName}{comma}");
                            }
                        }
                    }
                    
                    funcCode.AppendLine($"{indent}        }};");
                    
                    // 리스트 필드와 중첩된 객체 필드 처리 - 다음 단계 반복문 변수로 'm' 사용
                    GenerateNestedFieldsLoadCode(funcCode, tableFields, $"{indent}        ", $"{targetVar}.{fieldName}", $"fb{fieldName}Item.Value", "j");
                    
                    // 딕셔너리인 경우 키를 사용하여 저장
                    if (field.Value.IsList)
                    {
                        funcCode.AppendLine($"{indent}        {targetVar}.{fieldName}.Add(nested_item);");
                    }
                    else
                    {
                        funcCode.AppendLine($"{indent}        {targetVar}.{fieldName}[nested_item.Idx] = nested_item;");
                    }
                    
                    funcCode.AppendLine($"{indent}    }}");
                    funcCode.AppendLine($"{indent}}}");
                }
                else
                {
                    // 단일 커스텀 타입 필드 처리
                    funcCode.AppendLine();
                    funcCode.AppendLine($"{indent}// {fieldName} 로드");
                    funcCode.AppendLine($"{indent}var fb{fieldName} = {sourceVar}.{fieldName};");
                    funcCode.AppendLine($"{indent}if (fb{fieldName}.HasValue)");
                    funcCode.AppendLine($"{indent}{{");
                    
                    // 테이블 정보 가져오기
                    var tableInfo = GetExtractTableInfo(field.Value.Type);
                    Dictionary<string, FieldInfo> tableFields = ParseFields(tableInfo, false);
                    
                    // 각 필드에 대한 로드 코드 생성 - 리스트가 아닌 기본 필드만 이곳에서 처리
                    foreach (var tableField in tableFields)
                    {
                        if (!tableField.Value.IsList && !tableField.Value.IsDictionary && !tableField.Value.IsCustom)
                        {
                            string tableFieldName = ToUpperFirst(tableField.Value.Name);
                            
                            if (tableField.Value.IsReactive)
                            {
                                // ReactiveProperty는 Value에 값 할당
                                funcCode.AppendLine($"{indent}    {targetVar}.{fieldName}.{tableFieldName}.Value = fb{fieldName}.Value.{tableFieldName};");
                            }
                            else
                            {
                                funcCode.AppendLine($"{indent}    {targetVar}.{fieldName}.{tableFieldName} = fb{fieldName}.Value.{tableFieldName};");
                            }
                        }
                    }
                    
                    // 리스트 필드와 중첩된 객체 필드 처리 - 다음 단계 반복문 변수로 'm' 사용
                    GenerateNestedFieldsLoadCode(funcCode, tableFields, $"{indent}    ", $"{targetVar}.{fieldName}", $"fb{fieldName}.Value", "j");
                    
                    funcCode.AppendLine($"{indent}}}");
                }
            }
            // DateTime 타입 필드 처리
            else if (field.Value.IsDateTime)
            {
                funcCode.AppendLine();
                funcCode.AppendLine($"{indent}// {fieldName} DateTime 로드");
                funcCode.AppendLine($"{indent}{targetVar}.{fieldName} = new DateTime({sourceVar}.{fieldName});");
            }
            // 기본 타입 리스트 처리
            else if (field.Value.IsList || field.Value.IsDictionary)
            {
                funcCode.AppendLine();
                funcCode.AppendLine($"{indent}// {fieldName} 리스트 로드");
                funcCode.AppendLine($"{indent}{targetVar}.{fieldName}.Clear();");
                
                if (field.Value.Type.ToLower() == "string")
                {
                    // 문자열 리스트 처리
                    funcCode.AppendLine($"{indent}for (int j = 0; j < {sourceVar}.{fieldName}Length; j++)");
                    funcCode.AppendLine($"{indent}{{");
                    funcCode.AppendLine($"{indent}    string {fieldName.ToLower()}_val = {sourceVar}.{fieldName}(j);");
                    funcCode.AppendLine($"{indent}    {targetVar}.{fieldName}.Add({fieldName.ToLower()}_val);");
                    funcCode.AppendLine($"{indent}}}");
                }
                else
                {
                    // 다른 기본 타입 리스트 처리
                    funcCode.AppendLine($"{indent}for (int j = 0; j < {sourceVar}.{fieldName}Length; j++)");
                    funcCode.AppendLine($"{indent}{{");
                    funcCode.AppendLine($"{indent}    {field.Value.Type} {fieldName.ToLower()}_val = {sourceVar}.{fieldName}(j);");
                    funcCode.AppendLine($"{indent}    {targetVar}.{fieldName}.Add({fieldName.ToLower()}_val);");
                    funcCode.AppendLine($"{indent}}}");
                }
            }
        }
    }
    
    // 커스텀 타입 내의 중첩 필드들을 로드하는 헬퍼 메서드
    // loopVarName: 반복문에 사용할 인덱스 변수명 (재귀 호출 시 다른 변수명을 사용하기 위함)
    private static void GenerateNestedFieldsLoadCode(StringBuilder funcCode, Dictionary<string, FieldInfo> tableFields, string indent, string targetVar, string sourceVar, string loopVarName = "j")
    {
        // 다음 재귀 호출에서 사용할 반복문 인덱스 변수명 결정
        // string nextLoopVarName = loopVarName == "j" ? "m" : (loopVarName == "m" ? "n" : "p");

        // loopVarName의 다음 알파벳 문자를 nextLoopVarName으로 설정
        char currentChar = loopVarName[0];
        char nextChar = (char)(currentChar + 1);
        string nextLoopVarName = nextChar.ToString();

        
        foreach (var tableField in tableFields)
        {
            string fieldName = ToUpperFirst(tableField.Value.Name);
            
            if (tableField.Value.IsList || tableField.Value.IsDictionary)
            {
                // 리스트/딕셔너리 필드 처리
                funcCode.AppendLine();
                funcCode.AppendLine($"{indent}// {fieldName} 리스트 로드");
                funcCode.AppendLine($"{indent}{targetVar}.{fieldName}.Clear();");
                
                if (tableField.Value.Type.ToLower() == "string")
                {
                    // 문자열 리스트
                    funcCode.AppendLine($"{indent}for (int {nextLoopVarName} = 0; {nextLoopVarName} < {sourceVar}.{fieldName}Length; {nextLoopVarName}++)");
                    funcCode.AppendLine($"{indent}{{");
                    funcCode.AppendLine($"{indent}    string {fieldName.ToLower()}_val = {sourceVar}.{fieldName}({nextLoopVarName});");
                    funcCode.AppendLine($"{indent}    {targetVar}.{fieldName}.Add({fieldName.ToLower()}_val);");
                    funcCode.AppendLine($"{indent}}}");
                }
                else if (tableField.Value.IsCustom)
                {
                    // 커스텀 객체 리스트
                    funcCode.AppendLine($"{indent}for (int {nextLoopVarName} = 0; {nextLoopVarName} < {sourceVar}.{fieldName}Length; {nextLoopVarName}++)");
                    funcCode.AppendLine($"{indent}{{");
                    funcCode.AppendLine($"{indent}    var fbNestedItem = {sourceVar}.{fieldName}({nextLoopVarName});");
                    funcCode.AppendLine($"{indent}    if (fbNestedItem.HasValue)");
                    funcCode.AppendLine($"{indent}    {{");
                    funcCode.AppendLine($"{indent}        var nested_item = new {tableField.Value.Type}();");
                    
                    // 내부 객체의 테이블 정보 가져오기
                    var nestedTableInfo = GetExtractTableInfo(tableField.Value.Type);
                    Dictionary<string, FieldInfo> nestedFields = ParseFields(nestedTableInfo, false);
                    
                    // 내부 객체의 기본 필드 처리
                    foreach (var nestedField in nestedFields)
                    {
                        if (!nestedField.Value.IsList && !nestedField.Value.IsDictionary && !nestedField.Value.IsCustom)
                        {
                            string nestedFieldName = ToUpperFirst(nestedField.Value.Name);
                            funcCode.AppendLine($"{indent}        nested_item.{nestedFieldName} = fbNestedItem.Value.{nestedFieldName};");
                        }
                    }
                    
                    // 내부 객체의 리스트 필드 처리 (재귀) - 다음 변수명 전달
                    GenerateNestedFieldsLoadCode(funcCode, nestedFields, $"{indent}        ", "nested_item", "fbNestedItem.Value", nextLoopVarName);
                    
                    funcCode.AppendLine($"{indent}        {targetVar}.{fieldName}.Add(nested_item);");
                    funcCode.AppendLine($"{indent}    }}");
                    funcCode.AppendLine($"{indent}}}");
                }
                else
                {
                    // 기본 타입 리스트
                    funcCode.AppendLine($"{indent}for (int {nextLoopVarName} = 0; {nextLoopVarName} < {sourceVar}.{fieldName}Length; {nextLoopVarName}++)");
                    funcCode.AppendLine($"{indent}{{");
                    funcCode.AppendLine($"{indent}    {tableField.Value.Type} {fieldName.ToLower()}_val = {sourceVar}.{fieldName}({nextLoopVarName});");
                    funcCode.AppendLine($"{indent}    {targetVar}.{fieldName}.Add({fieldName.ToLower()}_val);");
                    funcCode.AppendLine($"{indent}}}");
                }
            }
            else if (tableField.Value.IsCustom)
            {
                // 내부 커스텀 객체 필드 처리
                funcCode.AppendLine();
                funcCode.AppendLine($"{indent}// {fieldName} 커스텀 객체 로드");
                funcCode.AppendLine($"{indent}var fb{fieldName} = {sourceVar}.{fieldName};");
                funcCode.AppendLine($"{indent}if (fb{fieldName}.HasValue)");
                funcCode.AppendLine($"{indent}{{");
                
                // 내부 객체의 테이블 정보 가져오기
                var nestedTableInfo = GetExtractTableInfo(tableField.Value.Type);
                Dictionary<string, FieldInfo> nestedFields = ParseFields(nestedTableInfo, false);
                
                // 내부 객체의 기본 필드 처리
                foreach (var nestedField in nestedFields)
                {
                    if (!nestedField.Value.IsList && !nestedField.Value.IsDictionary && !nestedField.Value.IsCustom)
                    {
                        string nestedFieldName = ToUpperFirst(nestedField.Value.Name);
                        funcCode.AppendLine($"{indent}    {targetVar}.{fieldName}.{nestedFieldName} = fb{fieldName}.Value.{nestedFieldName};");
                    }
                }
                
                // 내부 객체의 리스트 필드 처리 (재귀) - 다음 변수명 전달
                GenerateNestedFieldsLoadCode(funcCode, nestedFields, $"{indent}    ", $"{targetVar}.{fieldName}", $"fb{fieldName}.Value", nextLoopVarName);
                
                funcCode.AppendLine($"{indent}}}");
            }
            else if (tableField.Value.IsDateTime)
            {
                // DateTime 필드 처리
                funcCode.AppendLine();
                funcCode.AppendLine($"{indent}// {fieldName} DateTime 로드");
                funcCode.AppendLine($"{indent}{targetVar}.{fieldName} = new DateTime({sourceVar}.{fieldName});");
            }
        }
    }

    private static void CreateNewDataClassFile(string filePath, string className, Dictionary<string, FieldInfo> classFields)
    {
        StringBuilder classCode = new StringBuilder();
        classCode.AppendLine("using System;");
        classCode.AppendLine("using System.Collections.Generic;");
        classCode.AppendLine("using UniRx;");
        classCode.AppendLine("using Google.FlatBuffers;");
        classCode.AppendLine("");

        // UserDataSystem 클래스 생성
        classCode.AppendLine("public partial class UserDataSystem");
        classCode.AppendLine("{");
        classCode.AppendLine($"");
        classCode.AppendLine($"");
        classCode.AppendLine($"");
        classCode.AppendLine("}");
        classCode.AppendLine("");

        // 일반 클래스 생성
        classCode.AppendLine($"public class {className}");
        classCode.AppendLine("{");
        
        // 필드 추가 (중복 방지)
        Dictionary<string, bool> addedFields = new Dictionary<string, bool>();
        
        foreach (var fieldItem in classFields)
        {
            var fieldInfo = fieldItem.Value;
            string propertyName = ToUpperFirst(fieldInfo.Name);
            
            // 이미 추가된 필드는 건너뜀
            if (!addedFields.ContainsKey(propertyName.ToLower()))
            {
                AddFieldToClassCode(classCode, fieldInfo);
                addedFields[propertyName.ToLower()] = true;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"{className} 클래스 생성 중 중복 필드 발견: {propertyName} - 무시됨");
            }
        }
        
        classCode.AppendLine("");
        classCode.AppendLine("}");
        
        File.WriteAllText(filePath, classCode.ToString());
        UnityEngine.Debug.Log($"{className}.cs 파일이 생성되었습니다.");
    }


    // 필드 데이터 추가
    private static void AddFieldToClassCode(StringBuilder codeBuilder, FieldInfo fieldInfo, string indent = "    ")
    {
        string propertyName = ToUpperFirst(fieldInfo.Name);
        string propertyType = GetCSharpType(fieldInfo);
        
        if (fieldInfo.IsReactive)
        {
            codeBuilder.AppendLine($"{indent}public {propertyType} {propertyName} {{ get; set; }} = new ReactiveProperty<{GetBasicCSharpType(fieldInfo.Type)}>({GetDefaultValue(fieldInfo)});");
        }
        else if (fieldInfo.IsReactiveCollection)
        {
            codeBuilder.AppendLine($"{indent}public {propertyType} {propertyName} {{ get; set; }} = new ReactiveCollection<{GetBasicCSharpType(fieldInfo.Type)}>();");
        }
        else if (fieldInfo.IsList)
        {
            codeBuilder.AppendLine($"{indent}public List<{fieldInfo.Type}> {propertyName} = new List<{fieldInfo.Type}>();");
        }
        else if (fieldInfo.IsDictionary)
        {
            codeBuilder.AppendLine($"{indent}public Dictionary<int, {fieldInfo.Type}> {propertyName} = new Dictionary<int, {fieldInfo.Type}>();");
        }
        else if (fieldInfo.IsCustom)
        {
            codeBuilder.AppendLine($"{indent}public {fieldInfo.Type} {propertyName} = new {fieldInfo.Type}();");
        }
        else if (fieldInfo.IsDateTime)
        {
            codeBuilder.AppendLine($"{indent}public DateTime {propertyName} {{ get; set; }} = new DateTime();");
        }
        else
        {
            codeBuilder.AppendLine($"{indent}public {propertyType} {propertyName} {{ get; set; }} = {GetDefaultValue(fieldInfo)};");
        }
    }

    private static void AddSaveLoadDataFunc(string fileName , string funcName , string filter , bool addBuilder ){

        string userDataPath = Path.Combine(Application.dataPath, "Scripts", "Game", "Data", $"{fileName}.cs");
        if (!File.Exists(userDataPath)){
            Debug.LogError($"{fileName}.cs 파일을 찾을 수 없습니다. 경로: {userDataPath}");
            return;
        }

        // filter 위치 찾기
        string content = File.ReadAllText(userDataPath);
        int insertPosition = content.IndexOf(filter);
        if (insertPosition == -1)
        {
            Debug.LogError($"{fileName}.cs 파일에서 {filter} 위치를 찾을 수 없습니다.");
            return;
        }

        // 주석 다음 줄의 위치 찾기
        int nextLinePos = content.IndexOf('\n', insertPosition);

        string strBuilder = addBuilder ? "builder" : "";

        // filter 위치에서 funcName의 함수가 있는지 확인 후 없으면 추가
        if (!content.Contains(funcName))    
        {
            // 코드 삽입 (다음 줄의 시작 부분에)
            content = content.Insert(nextLinePos + 1, $"        {funcName}({strBuilder});\n");
            // 파일에 변경 내용 저장
            File.WriteAllText(userDataPath, content);
            Debug.Log($"{fileName}.cs 파일 {funcName} 함수에 코드가 추가되었습니다.");
        }


    }

    private static bool AddFindStringData(string fileName , string filter , string addString , bool isNextLine = true , string baseSpace = "    " , string lastSpace = ""){

        string userDataPath = Path.Combine(Application.dataPath, "Scripts", "Game", "Data", $"{fileName}.cs");
        if (!File.Exists(userDataPath)){
            return false;
        }

        // filter 위치 찾기
        string content = File.ReadAllText(userDataPath);
        int insertPosition = content.IndexOf(filter);
        if (insertPosition == -1)
        {
            return false;
        }
        
        if (addString == ""){ return true; }

        // 주석 다음 줄의 위치 찾기
        int linePos = insertPosition-1;
        if (isNextLine){
            linePos = content.IndexOf('\n', insertPosition);
        }

        // filter 위치에서 funcName의 함수가 있는지 확인 후 없으면 추가
        if (!content.Contains(addString))    
        {
            // 코드 삽입 (다음 줄의 시작 부분에)
            content = content.Insert(linePos + 1, $"{baseSpace}{addString}\n{lastSpace}");
            // 파일에 변경 내용 저장
            File.WriteAllText(userDataPath, content);
            Debug.Log($"{fileName}.cs 파일 {addString} 코드가 추가되었습니다.");
        }

        return true;
    }
















    // UserData_Client.cs 파일에 변수 추가부분
    private static void AddVariableToUserDataSystem(FieldInfo field)
    {
        string userDataPath = Path.Combine(Application.dataPath, "Scripts", "Game", "Data", "UserData_Client.cs");
        if (!File.Exists(userDataPath))
        {
            UnityEngine.Debug.LogError($"UserData_Client.cs 파일을 찾을 수 없습니다. 경로: {userDataPath}");
            return;
        }

        string content = File.ReadAllText(userDataPath);
        
        // 이미 변수가 존재하는지 확인
        string propertyName = ToUpperFirst(field.Name);
        string variablePattern = $@"public\s+{GetCSharpType(field)}\s+{propertyName}";
        if (Regex.IsMatch(content, variablePattern))
        {
            return;
        }

        // "@변수 자동 등록 위치" 주석 찾기
        int insertPosition = content.IndexOf("@변수 자동 등록 위치");
        
        // 주석을 찾지 못한 경우 클래스 시작 부분에 추가
        if (insertPosition == -1)
        {
            // 클래스 선언 부분 찾기
            Match classMatch = Regex.Match(content, @"public\s+partial\s+class\s+UserDataSystem\s*{");
            if (!classMatch.Success)
            {
                UnityEngine.Debug.LogError("UserDataSystem 클래스 선언을 찾을 수 없습니다.");
                return;
            }
            insertPosition = classMatch.Index + classMatch.Length;
        }

        // StringBuilder를 사용하여 필드 코드 생성
        StringBuilder fieldBuilder = new StringBuilder();
        AddFieldToClassCode(fieldBuilder, field, "");
        string variableDeclaration = fieldBuilder.ToString().TrimEnd();

        // 현재 위치에 코드 추가 (다음 라인부터 추가)
        int nextLinePos = content.IndexOf('\n', insertPosition);
        if (nextLinePos != -1)
        {
            content = content.Insert(nextLinePos + 1, "    " + variableDeclaration + "\n");
        }

        // 추가된 변수를 UserData_Client.cs파일의 ConnectReadOnlyDatas함수 하단에 FlatBuffers 값 대입하기
        insertPosition = content.IndexOf("@변수 자동 데이터 추가");
        if (insertPosition != -1){
            // 추가된 변수들 데이터 넣기
            string flatBufferFieldName = field.Name;
            string clientFieldName = ToUpperFirst(field.Name);
            
            // FlatBuffer 필드명 형식으로 변환 (언더스코어 제거하고 다음 문자 대문자로)
            string fbFieldName = ConvertToFlatBufferFieldName(flatBufferFieldName);
            
            // 변수 타입에 따라 적절한 코드 생성
            string dataAssignCode = "";
            
            if (field.IsReactive)
            {
                // Reactive 타입 (IReactiveProperty)
                dataAssignCode = $"{clientFieldName}.Value = flatBufferUserData.{fbFieldName};";
            }
            else if (field.IsReactiveCollection)
            {
                // ReactiveCollection 타입
                dataAssignCode = $"{clientFieldName}.Clear();\n        for (int i = 0; i < flatBufferUserData.{fbFieldName}Length; i++) {{\n            var item = flatBufferUserData.{fbFieldName}(i);\n            {clientFieldName}.Add(item);\n        }}";
            }
            else if (field.IsDateTime)
            {
                dataAssignCode = $"{clientFieldName} = new DateTime(flatBufferUserData.{fbFieldName});";
            }
            else if (field.IsCustom)
            {
                if (field.IsList)
                {    
                    dataAssignCode = $"{clientFieldName}.Clear();\n        if (flatBufferUserData.{fbFieldName} != null) {{\n            for (int i = 0; i < flatBufferUserData.{fbFieldName}Length; i++) {{\n                var item = flatBufferUserData.{fbFieldName}(i);\n                if (item.HasValue) {{\n                    {clientFieldName}.Add(new {field.Type}());\n                    // 여기서 필요에 따라 item의 속성들을 {field.Type}에 매핑\n                }}\n            }}\n        }}";
                }
                else if (field.IsDictionary){
                    dataAssignCode = $"{clientFieldName}.Clear();\n        if (flatBufferUserData.{fbFieldName} != null) {{\n            for (int i = 0; i < flatBufferUserData.{fbFieldName}Length; i++) {{\n                var item = flatBufferUserData.{fbFieldName}(i);\n                if (item.HasValue) {{\n                    int key = item.Value.Idx; // Dictionary 키 (보통 Idx 필드 사용)\n                    {clientFieldName}[key] = new {field.Type}();\n                    // 여기서 필요에 따라 item의 속성들을 {field.Type}에 매핑\n                }}\n            }}\n        }}";
                }
                else
                {
                    // 단일 커스텀 타입
                    dataAssignCode = $"if (flatBufferUserData.{fbFieldName}.HasValue) {{\n            {clientFieldName} = new {field.Type}();\n            // 여기서 필요에 따라 flatBufferUserData.{fbFieldName}.Value의 속성들을 {clientFieldName}에 매핑\n        }}";
                }
            }
            else
            {
                if (field.IsList)
                {    
                    StringBuilder addCode = new StringBuilder();
                    addCode.AppendLine($"{clientFieldName}.Clear();");
                    addCode.AppendLine($"        for (int i = 0; i < flatBufferUserData.{fbFieldName}Length; i++) {{");
                    addCode.AppendLine($"            var item = flatBufferUserData.{fbFieldName}(i);");
                    addCode.AppendLine($"            {clientFieldName}.Add(item);");
                    addCode.AppendLine($"        }}");

                    dataAssignCode = addCode.ToString();
                }
                else{
                    // 일반 타입
                    dataAssignCode = $"{clientFieldName} = flatBufferUserData.{fbFieldName};";
                }
            }
            
            // 현재 위치에 코드 추가 (다음 라인부터 추가)
            nextLinePos = content.IndexOf('\n', insertPosition);
            if (nextLinePos != -1)
            {
                content = content.Insert(nextLinePos + 1, "        " + dataAssignCode + "\n");
            }
        }

        // UserData.cs 파일에도 SaveFile 함수에 변수 저장 코드 추가
        AddSaveCodeToUserData(field);

        File.WriteAllText(userDataPath, content);
    }

    // UserData.cs 파일의 SaveFile 함수에 변수 저장 코드 추가하는 함수
    private static void AddSaveCodeToUserData(FieldInfo field)
    {
        string userDataPath = Path.Combine(Application.dataPath, "Scripts", "Game", "Data", "UserData.cs");
        if (!File.Exists(userDataPath))
        {
            UnityEngine.Debug.LogError($"UserData.cs 파일을 찾을 수 없습니다. 경로: {userDataPath}");
            return;
        }

        string content = File.ReadAllText(userDataPath);
        
        string clientFieldName = ToUpperFirst(field.Name);
        string fbFieldName = ConvertToFlatBufferFieldName(field.Name);
        string saveCode = "";
        string indent = "        "; // 들여쓰기 8칸(탭 대신 공백 사용)


        string strSaveStringField = "";
        

        // 변수 타입에 따른 저장 코드 생성
        if (field.IsReactive)
        {
            // Reactive 타입 (IReactiveProperty)
            if (field.Type.ToLower() == "string")
            {
                strSaveStringField = $"var str{fbFieldName} = builder.CreateString({clientFieldName}.Value);";
                saveCode = $"BanpoFri.Data.UserData.Add{fbFieldName}(builder, str{fbFieldName});";
            }
            else
            {
                saveCode = $"BanpoFri.Data.UserData.Add{fbFieldName}(builder, {clientFieldName}.Value);";
            }
        }
        else if (field.IsReactiveCollection || field.IsList)
        {
            // ReactiveCollection 타입 (배열/리스트)
            if (field.Type.ToLower() == "string")
            {
                saveCode = $"// {field.Name}\n" +
                          $"{indent}StringOffset[] {field.Name}Array = null;\n" +
                          $"{indent}if ({clientFieldName}.Count > 0) {{\n" +
                          $"{indent}    {field.Name}Array = new StringOffset[{clientFieldName}.Count];\n" +
                          $"{indent}    for (int i = 0; i < {clientFieldName}.Count; i++) {{\n" +
                          $"{indent}        {field.Name}Array[i] = builder.CreateString({clientFieldName}[i]);\n" +
                          $"{indent}    }}\n" +
                          $"{indent}}}\n" +
                          $"{indent}VectorOffset {field.Name}Vec = default(VectorOffset);\n" +
                          $"{indent}if ({field.Name}Array != null)\n" +
                          $"{indent}    {field.Name}Vec = BanpoFri.Data.UserData.Create{fbFieldName}Vector(builder, {field.Name}Array);\n";
                        //   $"{indent}BanpoFri.Data.UserData.Add{fbFieldName}(builder, {field.Name}Vec);";
            }
            else
            {
                saveCode = $"// {field.Name}\n" +
                          $"{indent}{field.Type}[] {field.Name}Array = null;\n" +
                          $"{indent}if ({clientFieldName}.Count > 0) {{\n" +
                          $"{indent}    {field.Name}Array = {clientFieldName}.ToArray();\n" +
                          $"{indent}}}\n" +
                          $"{indent}VectorOffset {field.Name}Vec = default(VectorOffset);\n" +
                          $"{indent}if ({field.Name}Array != null)\n" +
                          $"{indent}    {field.Name}Vec = BanpoFri.Data.UserData.Create{fbFieldName}Vector(builder, {field.Name}Array);\n";
                          //$"{indent}BanpoFri.Data.UserData.Add{fbFieldName}(builder, {field.Name}Vec);";
            }

            // 코드 삽입 위치를 // @add userdata 주석 위에 삽입
            int addUserDataIndex = content.IndexOf("// @add userdata");
            if (addUserDataIndex != -1)
            {
                // 코드 삽입
                content = content.Insert(addUserDataIndex, saveCode + $"\n{indent}");
                
            }

            saveCode = $"BanpoFri.Data.UserData.Add{fbFieldName}(builder, {field.Name}Vec);";

        }
        else if (field.IsDateTime)
        {
            saveCode = $"BanpoFri.Data.UserData.Add{fbFieldName}(builder, {clientFieldName}.Ticks);";
        }
        else
        {
            // 일반 타입
            if (field.Type.ToLower() == "string")
            {
                strSaveStringField = $"var str{fbFieldName} = builder.CreateString({clientFieldName});";
                saveCode = $"BanpoFri.Data.UserData.Add{fbFieldName}(builder, str{fbFieldName});";
            }
            else
            {
                saveCode = $"BanpoFri.Data.UserData.Add{fbFieldName}(builder, {clientFieldName});";
            }
        }



        if (strSaveStringField != ""){
            // 코드 삽입 위치를 // @add userdata 주석 위에 삽입
            int addUserDataIndex = content.IndexOf("// @add userdata");
            if (addUserDataIndex != -1)
            {
                // 코드 삽입
                content = content.Insert(addUserDataIndex, strSaveStringField + $"\n{indent}");
                
            }


        }



        int insertPosition = content.IndexOf("var orc = BanpoFri.Data.UserData.EndUserData(builder);");
        if (insertPosition == -1)
        {
            UnityEngine.Debug.LogError("UserData.cs 파일에서 EndUserData 호출을 찾을 수 없습니다.");
            return;
        }



        // 코드 삽입
        content = content.Insert(insertPosition, saveCode + $"\n{indent}");
        
        // 파일에 변경 내용 저장
        File.WriteAllText(userDataPath, content);
        
        UnityEngine.Debug.Log($"UserData.cs 파일에 {clientFieldName} 저장 코드가 추가되었습니다.");
    }

    private static Dictionary<string, FieldInfo> ParseFields(string userDataContent , bool ignoreCreateDataClass = true)
    {
        Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();
        
        if (ignoreCreateDataClass) {
            // @AutoCreate와 @EndCreate 주석을 처리하기 위한 변수들
            StringBuilder processedContent = new StringBuilder();
            bool isProcessing = false;
            
            // 줄 단위로 처리
            string[] lines = userDataContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            
            foreach (string line in lines) {
                // @AutoCreate 주석이 있으면 처리 시작
                if (line.Contains("@AutoCreate")) {
                    isProcessing = true;
                }
                // @EndCreate 주석이 있으면 처리 중단
                else if (line.Contains("@EndCreate")) {
                    isProcessing = false;
                }
                
                // 처리 중인 상태일 때만 내용 추가
                if (isProcessing) {
                    processedContent.AppendLine(line);
                }
            }
            
            // 필터링된 내용이 없으면 빈 결과 반환
            if (processedContent.Length == 0) {
                return fields;
            }
            
            userDataContent = processedContent.ToString();
        }
        
        // 필드명:타입(배열 여부 포함) = 기본값(선택적); // @ReactiveProperty 또는 다른 주석 옵션
        MatchCollection fieldMatches = Regex.Matches(userDataContent, @"(\w+)\s*:\s*(?:\[)?(\w+)(?:\])?\s*(?:=\s*[^;]+)?\s*;(?:\s*//\s*(@\w+))?", RegexOptions.Singleline);
        
        foreach (Match match in fieldMatches)
        {
            string fieldName = match.Groups[1].Value;
            string fieldType = match.Groups[2].Value;
            bool isList = match.Value.Contains("[");
            
            // 주석에서 타입 힌트 확인
            string typeHint = match.Groups.Count > 3 && match.Groups[3].Success ? match.Groups[3].Value : string.Empty;
            bool isReactive = typeHint.Equals("@ReactiveProperty", StringComparison.OrdinalIgnoreCase);
            bool isReactiveCollection = typeHint.Equals("@ReactiveCollection", StringComparison.OrdinalIgnoreCase);
            bool isDictionary = typeHint.Equals("@Dictionary", StringComparison.OrdinalIgnoreCase);

            if (isDictionary){ isList = false; }

            // 기본 타입인지 커스텀 타입인지 확인
            bool isCustomType = !IsBasicType(fieldType);

            // long타입의 DateTime인지 확인
            bool isDateTime = typeHint.Equals("@DateTime", StringComparison.OrdinalIgnoreCase);
            
            FieldInfo fieldInfo = new FieldInfo
            {
                Name = fieldName,
                Type = fieldType,
                IsReactive = isReactive,
                IsReactiveCollection = isReactiveCollection,
                IsList = isList,
                IsDictionary = isDictionary,
                IsCustom = isCustomType,
                IsDateTime = isDateTime
            };

            fields[fieldName] = fieldInfo;
        }
        
        return fields;
    }


    // UserData.fbs 파일 내부에서 필요한 table 정보 추출
    private static string GetExtractTableInfo(string tableName)
    {
        string fbsContent = "";
        if (FBS_CONTENT == "")
        {
            string fbsPath = Path.Combine(Application.dataPath, "..", FBS_PATH);
            if (!File.Exists(fbsPath))
            {
                UnityEngine.Debug.LogError($"UserData.fbs 파일을 찾을 수 없습니다. 경로: {fbsPath}");
                return string.Empty;
            }
            FBS_CONTENT = File.ReadAllText(fbsPath);
        }
        
        fbsContent = FBS_CONTENT;
        // 지정된 테이블 찾기
        Match tableMatch = Regex.Match(fbsContent, $@"table\s+{tableName}\s*{{([^}}]*)}}");
        if (!tableMatch.Success)
        {
            UnityEngine.Debug.LogError($"UserData.fbs 파일에서 {tableName} 테이블을 찾을 수 없습니다.");
            return string.Empty;
        }

        string tableContent = tableMatch.Groups[1].Value;
        
        // 각 줄을 분리
        string[] lines = tableContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        StringBuilder result = new StringBuilder();
        
        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            // 주석으로 시작하지만 필드 정의 형태를 가진 경우만 제외
            if (trimmedLine.StartsWith("//"))
            {
                // 필드 패턴(예: field:type;)이 있는지 확인
                string commentContent = trimmedLine.Substring(2).Trim();
                // 주석 내용이 필드 정의인지 확인 (필드명:타입; 패턴)
                if (Regex.IsMatch(commentContent, @"^\w+\s*:\s*\w+\s*;"))
                {
                    // 필드 정의 주석은 제외
                    continue;
                }
                // 특별한 의미의 주석(@로 시작하는 등)은 유지
            }
            else if (trimmedLine.StartsWith("/*"))
            {
                // 여러 줄 주석 내에 필드 정의가 있는지 확인
                string commentContent = trimmedLine.Substring(2).TrimEnd('*', '/').Trim();
                if (Regex.IsMatch(commentContent, @"^\w+\s*:\s*\w+\s*;"))
                {
                    // 필드 정의 주석은 제외
                    continue;
                }
            }
            
            // 그 외 모든 줄은 유지
            result.AppendLine(trimmedLine);
        }
        
        return result.ToString();
    }



    // 기본 타입인지 확인하는 메서드
    private static bool IsBasicType(string typeName)
    {
        // FlatBuffers 기본 타입들
        string[] basicTypes = new string[] 
        { 
            "bool", "byte", "ubyte", "short", "ushort", "int", "uint", "float", "long", "ulong", "double", 
            "int8", "uint8", "int16", "uint16", "int32", "uint32", "int64", "uint64", "float32", "float64", "string"
        };
        
        return Array.IndexOf(basicTypes, typeName.ToLower()) >= 0;
    }

    // 앞글자 대문자로 변환 함수
    private static string ToUpperFirst(string str)
    {
        if (string.IsNullOrEmpty(str)){ return str; }
        return char.ToUpper(str[0]) + str.Substring(1);
    }
    
    
    private class FieldInfo
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsReactive { get; set; }
        public bool IsReactiveCollection { get; set; }
        public bool IsList { get; set; }
        public bool IsDictionary { get; set; }
        public bool IsCustom { get; set; }
        public bool IsDateTime { get; set; }
    }

    // FlatBuffer 필드명 형식으로 변환 (snake_case -> CamelCase)
    private static string ConvertToFlatBufferFieldName(string fieldName)
    {
        StringBuilder result = new StringBuilder();
        bool capitalizeNext = false;
        
        for (int i = 0; i < fieldName.Length; i++)
        {
            if (fieldName[i] == '_')
            {
                capitalizeNext = true;
            }
            else
            {
                if (capitalizeNext)
                {
                    result.Append(char.ToUpper(fieldName[i]));
                    capitalizeNext = false;
                }
                else
                {
                    result.Append(fieldName[i]);
                }
            }
        }
        
        // 첫 글자는 항상 대문자로 (FlatBuffer 규칙)
        if (result.Length > 0)
        {
            result[0] = char.ToUpper(result[0]);
        }
        
        return result.ToString();
    }

    private static string GetCSharpType(FieldInfo field)
    {
        if (field.IsReactive)
        {
            return $"IReactiveProperty<{GetBasicCSharpType(field.Type)}>";
        }
        else if (field.IsReactiveCollection)
        {
            return $"IReactiveCollection<{GetBasicCSharpType(field.Type)}>";
        }
        else if (field.IsDictionary)
        {
            return $"Dictionary<int, {GetBasicCSharpType(field.Type)}>";
        }
        else if (field.IsList)
        {
            return $"List<{GetBasicCSharpType(field.Type)}>";
        }
        else if (field.IsDateTime)
        {
            return "DateTime";
        }
        return GetBasicCSharpType(field.Type);
    }

    private static string GetBasicCSharpType(string flatBufferType)
    {
        switch (flatBufferType.ToLower())
        {
            case "bool": return "bool";
            case "byte": return "byte";
            case "ubyte": return "byte";
            case "short": return "short";
            case "ushort": return "ushort";
            case "int": return "int";
            case "uint": return "uint";
            case "float": return "float";
            case "long": return "long";
            case "ulong": return "ulong";
            case "double": return "double";
            case "int8": return "sbyte";
            case "uint8": return "byte";
            case "int16": return "short";
            case "uint16": return "ushort";
            case "int32": return "int";
            case "uint32": return "uint";
            case "int64": return "long";
            case "uint64": return "ulong";
            case "float32": return "float";
            case "float64": return "double";
            case "string": return "string";
            default: return flatBufferType;
        }
    }

    private static string GetDefaultValue(FieldInfo field)
    {
        switch (field.Type.ToLower())
        {
            case "bool": return "false";
            case "string": return "\"\"";
            case "int":
            case "int32":
            case "uint":
            case "uint32":
            case "short":
            case "ushort":
            case "int16":
            case "uint16": return "0";
            case "float":
            case "float32":
            case "double":
            case "float64": return "0.0f";
            case "long":
            case "int64":
            case "ulong":
            case "uint64": return "0";
            default: return "default";
        }
    }

    // 닫는 중괄호 앞에 빈 줄이 있는지 확인하는 함수
    private static bool HasEmptyLineBeforeBrace(string code, int bracePos)
    {
        int checkPos = bracePos - 2; // 닫는 중괄호 바로 앞 문자
        bool hasEmptyLine = false;
        
        // 닫는 중괄호 앞에 빈 줄이 있는지 확인
        while (checkPos >= 0 && (code[checkPos] == '\r' || code[checkPos] == '\n' || code[checkPos] == ' ' || code[checkPos] == '\t'))
        {
            if (code[checkPos] == '\n')
            {
                hasEmptyLine = true;
                break;
            }
            checkPos--;
        }
        
        return hasEmptyLine;
    }

    // string에서 특정 함수와 그 내용들 전부 삭제
    private static string RemoveFunctionAndContents(string code, string functionPattern)
    {
        // 정규식으로 함수 선언 찾기
        Match funcMatch = Regex.Match(code, functionPattern);
        if (!funcMatch.Success)
        {
            return code; // 함수가 없으면 원본 코드 반환
        }

        // 함수 선언 시작 위치
        int funcStartPos = funcMatch.Index;
        
        // 함수 앞의 불필요한 공백과 줄바꿈 제거
        int leadingSpacePos = funcStartPos - 1;
        while (leadingSpacePos >= 0 && (code[leadingSpacePos] == ' ' || code[leadingSpacePos] == '\t' || code[leadingSpacePos] == '\r' || code[leadingSpacePos] == '\n'))
        {
            leadingSpacePos--;
        }
        leadingSpacePos++; // 공백이 아닌 문자 바로 다음 위치로 이동
        
        // 함수 본문 시작 위치 (첫 번째 중괄호 뒤)
        int openBracePos = code.IndexOf('{', funcStartPos);
        if (openBracePos == -1)
        {
            return code; // 중괄호가 없으면 원본 코드 반환
        }
        
        // FindClassEndPosition 함수를 사용하여 함수 끝 위치 찾기
        int pos = FindClassEndPosition(code, openBracePos);
        if (pos == -1)
        {
            return code; // 함수 끝을 찾지 못했으면 원본 코드 반환
        }
        
        // 닫는 중괄호 다음 위치
        int endPos = pos + 1;
        
        // 함수와 그 내용 삭제 (앞의 공백, 닫는 중괄호 포함)
        return code.Remove(leadingSpacePos, endPos - leadingSpacePos);
    }

    
    // 기존 필드 추출
    private static HashSet<string> ExtractExistingFields(string code)
    {
        HashSet<string> fields = new HashSet<string>();
        // 속성(Property) 형식의 필드 검출 (get; set;)
        foreach (Match fieldMatch in Regex.Matches(code, @"public\s+(?:IReactiveProperty<[\w<>]+>|IReactiveCollection<[\w<>]+>|List<\w+>|Dictionary<\w+,\s*\w+>|\w+)\s+(\w+)\s*{\s*get;\s*set;\s*}"))
        {
            if (fieldMatch.Groups.Count > 1)
            {
                fields.Add(fieldMatch.Groups[1].Value.ToLower());
            }
        }
        
        // 필드 형식 검출 (명시적 필드)
        foreach (Match fieldMatch in Regex.Matches(code, @"public\s+(?:IReactiveProperty<[\w<>]+>|IReactiveCollection<[\w<>]+>|List<\w+>|Dictionary<\w+,\s*\w+>|\w+)\s+(\w+)\s*="))
        {
            if (fieldMatch.Groups.Count > 1)
            {
                fields.Add(fieldMatch.Groups[1].Value.ToLower());
            }
        }
        
        // 자동 속성이 아닌 일반 필드도 검출
        foreach (Match fieldMatch in Regex.Matches(code, @"public\s+(?:IReactiveProperty<[\w<>]+>|IReactiveCollection<[\w<>]+>|List<\w+>|Dictionary<\w+,\s*\w+>|\w+)\s+(\w+);"))
        {
            if (fieldMatch.Groups.Count > 1)
            {
                fields.Add(fieldMatch.Groups[1].Value.ToLower());
            }
        }
        
        return fields;
    }

    
    // 클래스 끝 위치 찾기
    private static int FindClassEndPosition(string code, int startPos)
    {
        int endPos = startPos;
        int braceCount = 1; // 시작 중괄호 하나는 이미 포함
        
        while (braceCount > 0 && endPos < code.Length)
        {
            endPos++;
            if (endPos >= code.Length)
                return -1;
                
            if (code[endPos] == '{')
                braceCount++;
            else if (code[endPos] == '}')
                braceCount--;
        }
        
        return braceCount == 0 ? endPos : -1;
    }
    
    // 클래스명으로 클래스의 끝 위치 찾기
    private static int FindClassEndPositionByName(string code, string className, bool isPartial = false)
    {
        string classPattern = isPartial ? 
            $@"public\s+partial\s+class\s+{className}\s*{{" : 
            $@"public\s+class\s+{className}\s*{{";
            
        Match classMatch = Regex.Match(code, classPattern);
        if (!classMatch.Success)
        {
            UnityEngine.Debug.LogError($"{className} 클래스 정의를 찾을 수 없습니다.");
            return -1;
        }
        
        int openBracePos = code.IndexOf('{', classMatch.Index);
        if (openBracePos == -1)
        {
            UnityEngine.Debug.LogError($"{className} 클래스의 시작 중괄호를 찾을 수 없습니다.");
            return -1;
        }
        
        return FindClassEndPosition(code, openBracePos);
    }
    
    // 클래스 타입 가져오기
    private static string GetClassType(FieldInfo field, string className)
    {
        if (field.IsList || field.IsDictionary)
        {
            return field.IsList ?  $"List<{className}>" : $"Dictionary<int, {className}>";
        }
        return className;
    }

}


