Dim allContainers as Array[Container]
Dim vr_containers as Array[Container]
Dim entries as Array[String]
Dim pauseState as Boolean
Dim vr_field_containers as Array[Container]

Sub OnInit()
	RegisterPluginVersion(0, 1, 0)
	RegisterInfoText("This plugin enables tracking of on screen time for graphics in the scene")

	Scene.Map.RegisterChangedCallback("tcp_status")
	allContainers = FindAllSceneContainers()
	printContainerNames(allContainers)
	pauseState = True
	
	vr_containers = FindAllContainersWithTextures(allContainers)
	
	if allContainers.ubound == -1 Then Exit Sub
	dim containersString as String = allContainers[0].Name
	For i = 1 To vr_containers.ubound
		if vr_containers[i] <> NULL Then
			containersString = containersString&","&vr_containers[i].Name
		End if	
	Next
	Dim tcpString as String = prepareJsonString("container_names", containersString)
	SendDataTCP(tcpString)
	

	Println("VR Containers")
	for i=0 to vr_containers.ubound
		entries.push(vr_containers[i].Name)
		println(vr_containers[i])
	Next
	
	UpdateGuiParameterEntries("vr_containers", entries)
	SendGuiRefresh()
	
End Sub

sub OnInitParameters()
	RegisterParameterString("remote_ip", "IP Address", "127.0.0.1", 16 , 16 , "")
	RegisterParameterInt("port", "Port", 999, 600, 60000)	
	RegisterParameterSliderDouble("visiblity_limit","Visible Containers Limit", 0.3, 0.0, 1.0, 100)
	RegisterParameterInt("camera_number", "Camera", 1, 1, 20)
	RegisterPushButton("pause_btn", "Start/Stop Tracking", 2)
	RegisterParameterLabel("pause_state_label", "Tracking", 100, 10)
	Dim arr as Array[String] 
	arr.push("Hello")
	arr.push("World")
	RegisterParameterList("vr_containers", "Tracked Containers", 0, arr, 100, 200)
	RegisterParameterString("addition_containers", "Additional Containers", "", 100, 1024, "")
End Sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if mapKey.Match("TcpSendAsync") Then
		println("TCP status received")
	End if
end sub

sub OnExecPerField()
' Check if there are any additional Containers we need to add
	If Not pauseState Then	
		AdditionalContainers()
		SendVisibleContainers()
	End If
	
	Dim vr_conts as Array[Container] = FindAllFieldContainers(vr_containers)
	Dim whz as Vertex = CalculateFieldWH(vr_conts)
	
	Println(whz)
end sub

sub OnExecAction(buttonId As Integer)
	If buttonId == 2 Then
		If pauseState Then
			pauseState = False
			SendGuiParameterShow("pause_state_label", SHOW)
		Else
			Println("Tracking Paused")
			PauseState = True
			SendGuiParameterShow("pause_state_label", HIDE)
		End If 
	End If
end sub


Function FindAllFieldContainers(vr_containers as Array[Container]) As Array[Container]
	Dim cont as Container
	Dim result as Array[Container]
	for i=0 to vr_containers.ubound
		cont  = vr_containers[i]
		
		If cont <> NULL Then
			Dim v as Vertex= cont.Position.xyz
			If v.x <> 0  or v.y <> 0 or v.z <> 0 Then
				result.push(cont)
			End If
		End If
	Next
	FindAllFieldContainers = result
End Function

Function CalculateFieldWH(field_containers as Array[Container]) as Vertex
	Dim minX, minY, maxX, maxY, x, y as Double
	Dim cont as Container
	Dim result as Vertex
	minX = 100000000000.0
	minY = 100000000000.0
	
	maxX = -100000000000.0
	maxY = -100000000000.0
	
	for i = 0 to field_containers.ubound
		cont  = field_containers[i]
		
		if cont <> NULL Then
			
			If min(cont.Position.x, minX) == cont.Position.x Then
				minX = cont.Position.x
			ElseIf max(cont.Position.x, maxX) == cont.Position.x Then
				maxX = cont.Position.x
			End If
			
			If min(cont.Position.y, minY) == cont.Position.y Then
				minY = cont.Position.y
			ElseIf max(cont.Position.y, maxY) == cont.Position.y Then
				maxY = cont.Position.y
			End If
		End if
		
	Next
	
	result.x = maxX - minX
	result.y = maxY - minY
	result.z = 0
	
	CalculateFieldWH = result

End Function

Sub SendVisibleContainers()
	Dim visConts as Array[String] = testPerObjectVisibility(vr_containers)
	Dim tcpString, containersString as String 
	
	
	if visConts.ubound == -1 Then Exit Sub	
		
	containersString = visConts[0]
	for i=1 To visConts.ubound
		containersString = containersString&","&visConts[i]
	Next
	
	tcpString = "vis_con:"&containersString
	SendDataTCP(tcpString)

End Sub

Sub SendDataTCP(data as String)
	Dim ip as String
	Dim port as Integer = 999
	Dim camera_n as Integer = 1
	
	ip = GetParameterString("remote_ip")
	port = GetParameterInt("port")
	camera_n = GetParameterInt("camera_number")
	
	data = data&":"&Scene.Name&":camera:"&CStr(camera_n)
	System.TcpSendAsync("tcp_status",ip, port, data, 10)
End Sub

Function FindAllContainersWithTextures(allConts as Array[Container]) as Array[Container]
	Dim result as Array[Container]
	Dim current as Container
	Println("\n\nPrinting Contianers with Textures\n\n\n")
	for i=0 to allConts.ubound
		current = allConts[i]
		if current <> NULL and current.Texture <> Null and current.Active Then
			Println(current.Name)
			result.push(current)
		End If
	Next
	FindAllContainersWithTextures = result 
End Function


Function prepareJsonString(func as String, data as String) as String
	prepareJsonString = func&":"&data
End Function

Function MatchContainerByName(contName as String, conts as Array[Container]) as Container
		Dim i as Integer = 0
		for i = 0 To conts.ubound
			Dim currCont as Container = conts[i]
			if currCont <> NULL and currCont.Name.Match(contName) Then
				MatchContainerByName = currCont
				Exit Function
			End If
		Next
End Function


Function ContainerTracked(cont as Container, allConts as Array[Container]) as Boolean
	for i=0 to allConts.ubound
		if cont == allConts[i] Then 
			ContainerTracked = True
			Exit Function
		End If	
	Next
	ContainerTracked = False
End Function


Sub AdditionalContainers()
	Dim contNames as String = GetParameterString("addition_containers")	
	Dim contNamesSplit as Array[String]
	Dim current as String
	
	contNames.Split(",", contNamesSplit)
	
	If  contNamesSplit.ubound == -1 Then
		Exit Sub
	End If
	Dim cont as Container
	for i=0 to contNamesSplit.ubound
		current = contNamesSplit[i]
		cont  = MatchContainerByName(current, allContainers)
		if ContainerTracked(cont, vr_containers) <> True and cont <> Null Then
			vr_containers.push(cont)
		End If
	Next

End Sub

Function testPerObjectVisibility(conts as Array[Container]) as Array[String]
	Dim i as Integer = 0
	Dim visCount as Integer = 0
	Dim contPerc as Double
	Dim result as Array[String]
	Dim visibilityLimit as Double = GetParameterDouble("visiblity_limit")
	
	For i = 0 To conts.ubound
		if conts[i] <> NULL Then
			contPerc = isContainerOnScreen(conts[i])
			if contPerc > visibilityLimit Then
				result.push(conts[i].Name)
			End if
		End If
	Next 
	testPerObjectVisibility = result
End Function

Function FindAllSceneContainers() as Array[Container]
	Dim rootCont as Container = Scene.RootContainer
	Dim resultsQ as Array[Container]
	Dim currentIndex as Integer = 0
	Dim currentContainer as Container
	Dim i as Integer = 0
	
	resultsQ.push(rootCont)
	currentContainer = rootCont
	
	' Collect all top level containers that are not child to the root container.
	Do
		currentContainer = currentContainer.NextContainer
		if currentContainer == Null Then Exit Do	
		resultsQ.push(currentContainer)
	Loop
	
	' Now we collect all children of the Scene using breadth first search
	Do 
		currentContainer = resultsQ[currentIndex]
		Dim childCount as Integer  = currentContainer.ChildContainerCount
		
		for i = 0 To childCount	
			if currentContainer.GetChildContainerByIndex(i) <> NULL Then
				resultsQ.push(currentContainer.GetChildContainerByIndex(i))
			End If
		Next

		currentIndex = currentIndex + 1
		
		If currentIndex > resultsQ.ubound Then Exit Do
	Loop
	
	FindAllSceneContainers = resultsQ

End Function

Sub printContainerNames(conts as Array[Container])
	Println("Found Objects Count: ")
	Println(conts.size)
	For i=0 To conts.ubound 
		Println(conts[i].Name)
	Next
End Sub

Function isContainerOnScreen(cont as Container) as Double
	' This function takes a container and checks if it's showing up in view
	Dim result as Double = 0.0
	Dim renderWidth, renderHeight as Integer
	Dim bl, tr, vBl, vTr as Vertex
	cont.GetScreenBounds(bl, tr)
	renderWidth = System.RenderWindowWidth
	renderHeight = System.RenderWindowHeight
	
	vBl = bl
	vTr = tr
	
	' Fully on screen
	if (bl.x >= 0 and bl.y >=0) and (tr.x <= renderWidth and tr.y <= renderHeight) Then
		isContainerOnScreen = 1.0
		Exit Function
	End If
	
	' Fully off screen
	if tr.x <= 0 or tr.y <= 0 then
		isContainerOnScreen = 0.0
		Exit Function
	End If
	
	if bl.x >=renderWidth or bl.y >= renderHeight then
		isContainerOnScreen = 0.0
		Exit Function
	End If
	
	' There's some area showing here..
	Dim totalArea, visibleArea as Double
	
	' Total Area of the Container
	totalArea = (tr.x - bl.x) * (tr.y - bl.y)
	
	if  bl.x < 0 Then
		vBl.x = 0
	End if
	
	if bl.y < 0 Then
		vBl.y = 0
	End if
	
	if tr.x > renderWidth Then
		vTr.x = renderWidth
	End if
	
	if tr.y > renderHeight Then
		vTr.y = renderHeight
	End If
	
	visibleArea = (vTr.x - vBl.x) * (vTr.y - vBl.y)
	isContainerOnScreen = visibleArea/totalArea
End Function


