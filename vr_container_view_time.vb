
Sub OnInit()
	Scene.Map.RegisterChangedCallback("tcp_status")
	Dim allContainers as Array[Container] = FindAllSceneContainers()
	printContainerNames(allContainers)
	if allContainers.ubound == -1 Then Exit Sub
	dim containersString as String = allContainers[0].Name
	For i = 1 To allContainers.ubound
		if allContainers[i] <> NULL Then
			containersString = containersString &","&allContainers[i].Name
		End if	
	Next
	Dim tcpString as String = prepareJsonString("container_names", containersString)
	SendDataTCP(tcpString)
	
End Sub


Sub SendDataTCP(data as String)
	Dim ip as String
	Dim port as Integer = 999
	
	ip = GetParameterString("remote_ip")
	port = GetParameterInt("port")
	data = data&":"&Scene.Name
	System.TcpSendAsync("tcp_status",ip, port, data, 10)
	
End Sub

Function prepareJsonString(func as String, data as String) as String
	prepareJsonString = func&":"&data
End Function

Sub SendVisibleContainers()
	Dim visConts as Array[String] = testPerObjectVisibility(FindAllSceneContainers())
	Dim tcpString, containersString as String 
	
	
	if visConts.ubound == -1 Then Exit Sub	
		
	containersString = visConts[0]
	for i=1 To visConts.ubound
		containersString = containersString&","&visConts[i]
	Next
	
	tcpString = "vis_con:"&containersString
	SendDataTCP(tcpString)

End Sub


sub OnExecPerField()
	SendVisibleContainers()
end sub

Function MatchContainerByName(conts as Array[Container]) as Container
		Dim i as Integer = 0
	
		for i = 0 To conts.ubound
		
			Dim currCont as Container = conts[i]
			
			if currCont <> NULL and currCont.Name.Match("img_centre") Then
				MatchContainerByName = currCont
				Exit Function
			End If
		Next
End Function

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


sub OnInitParameters()
	RegisterParameterString("remote_ip", "IP Address", "127.0.0.1", 16 , 16 , "")
	RegisterParameterInt("port", "Port", 999, 600, 60000)
	RegisterParameterSliderDouble("visiblity_limit","Visible Containers Limit", 0.3, 0.0, 1.0, 100)
end sub

sub OnSharedMemoryVariableChanged(map As SharedMemory, mapKey As String)
	if mapKey.Match("TcpSendAsync") Then
		println("TCP status received")
	End if
end sub
