Dim fileName = "C:\\ProgramData\\vizrt" & "\\" & Scene.Name & ".txt"
Dim omoPlugin as PluginInstance
Dim previousValue as Integer  = -1
Function TransformsToString() as String
	Dim result  as String = ""
	Dim rootContainer as Container = Scene.RootContainer
	Dim positionS as String = CStr(rootContainer.Position.x)&","&CStr(rootContainer.Position.y)&","&CStr(rootContainer.Position.z)
	Dim scalingS as String = CStr(rootContainer.Scaling.x)&","&CStr(rootContainer.Scaling.y)&","&CStr(rootContainer.Scaling.z)	
	Dim rotationS as String = CStr(rootContainer.Rotation.x)&","&CStr(rootContainer.Rotation.y)&","&CStr(rootContainer.Rotation.z)
	result = positionS&"\n"&ScalingS&"\n"&rotationS
	TransformsToString = result	
End Function


Sub SaveTransformsToFile()
	Dim transString as String = TransformsToString()
	System.SaveTextFile(fileName, transString)
End Sub

Sub LoadTransformFromFile()
	If System.FileExists(fileName) <> True Then
		Println("File with name: ")
		println(fileName)
		println("Does not exists!")
		Exit Sub
	end If
	
	Dim transString as Array[String] = LoadFile(fileName)
	StringsToTransforms(transString)
End Sub


Sub StringsToTransforms(str as Array[String])
	Dim pos as Array[Double] = LoadVector(str[0])
	Dim scale as Array[Double] = LoadVector(str[1])
	Dim rot as Array[Double] = LoadVector(str[2])
	
	Dim rootCont = Scene.RootContainer
	rootCont.Position.x = pos[0]
	rootCont.Position.y = pos[1]
	rootCont.Position.z = pos[2]
	
	rootCont.Scaling.x = scale[0]
	rootCont.Scaling.y = scale[1]
	rootCont.Scaling.z = scale[2]
	
	rootCont.Rotation.x = rot[0]
	rootCont.Rotation.y = rot[1]
	rootCont.Rotation.z = rot[2]
	
End Sub

Function LoadVector(vectString as String) as Array[Double]
	dim result as Array[String]
	vectString.Split(",", result)
	
	Dim x as double = CDbl(result[0])
	Dim y as double = CDbl(result[1])
	Dim z as double = CDbl(result[2])

	Dim coordinates as Array[Double]
	coordinates.Push(x)
	coordinates.Push(y)
	coordinates.Push(z)	
	LoadVector = coordinates
end Function

Function LoadFile(fileName as String) as Array[String]
	Dim result as String	
	System.LoadTextFile(fileName, result)
	Dim ret as Array[String]
	result.Split("\n", ret)
	LoadFile = ret
End Function


sub OnInitParameters()
	RegisterPushButton("save_btn", "save", 0)
	RegisterPushButton("load_btn", "reset", 1)
end sub


sub OnExecAction(buttonId As Integer)
	if buttonId == 0 then
		SaveTransformsToFile()
		println("State Saved to File: ")
		println(fileName)
	ElseIf buttonId == 1 then
		LoadTransformFromFile()
		println("State Loaded From File:")
		println(fileName)
	end If
end sub

sub OnInit()
	Println(Scene.RootContainer.Name)
	println(fileName)
	omoPlugin = this.GetFunctionPluginInstance("Omo")
	println("Omo Plugin Instance")
	Println(omoPlugin)
	if omoPlugin <> NULL then
		println("Omo Plugin Attached")
	else
		println("Please attached a plugin to the container")
	End if
end sub

sub OnExecPerField()
	if omoPlugin == NULL then
		Exit sub
	End if
	
	Dim currentValue as Integer = omoPlugin.GetParameterInt("vis_con")
	if currentValue <> previousValue Then
		if currentValue == 1 then
			LoadTransformFromFile()
		end if
		previousValue = 0
		omoPlugin.SetParameterInt("vis_con", 0)
	end if
	
end sub
