Dim markedContainers as Array[Container]
Dim baseGeo as Geometry
Dim width, height as Double
Dim contWidth, contHeight as Double
Dim currentNumberOfActiveContainers as Integer


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

Function GetAllMarkedContainers(sceneContainers as Array[Container]) as Array[Container]
		Dim currentCont as Container
		Dim result as Array[Container]
		
		For Each cont In sceneContainers
			Dim ret as Boolean = cont.Script.isMarked()
			If ret Then
				result.push(cont)
			End If 
		Next
	GetAllMarkedContainers = result
End Function

' For update direction, -1-go down, 0-remain the same, 1-go up
Function UpdateDirection(markedContainers as Array[Container]) as Integer
	Dim result as Integer = 0
	Dim activeContainersCount as Integer = 0
	
	For Each cont In markedContainers
		if cont.Active Then
			activeContainersCount +=1
		End if
	Next
	
	Dim diff as Integer = activeContainersCount - currentNumberOfActiveContainers
	
	If diff > 0 Then
		result = 1
	ElseIf diff < 0 Then
		result = -1
	Else
		result = 0
	End If
	
	currentNumberOfActiveContainers = activeContainersCount
	UpdateDirection = result
End Function

Sub UpdateBaseNoggieHeight()
	Dim dir as Integer = UpdateDirection(markedContainers)
	If dir == 0 Then
		Exit Sub
	End If
	
	If contHeight == -1.0 Then
		Exit Sub
	End If
	
	height = baseGeo.GetParameterDouble("height")
	' Containers reduced
	If dir == -1 Then
		If height - contHeight < 0 Then
			Exit Sub
		End If
		height = height - contHeight
		baseGeo.SetParameterDouble("height", height)
	Else
		height = height + contHeight
		baseGeo.SetParameterDouble("height", height)
	End If 
End Sub


sub OnInit()

	Dim sceneConts as Array[Container] = FindAllSceneContainers()
	markedContainers = GetAllMarkedContainers(sceneConts)
	baseGeo = this.Geometry
	
	width = -1.0
	height = -1.0
	contWidth = -1.0
	contHeight = -1.0
	
	currentNumberOfActiveContainers = markedContainers.ubound
	
	If baseGeo.Name.Match("Noggi") Then
		width = baseGeo.GetParameterDouble("width")
		height = baseGeo.GetParameterDouble("height")
	End If
	
	If markedContainers.ubound <> -1 Then
		Dim cont as Container = markedContainers[0]
		If cont.Geometry.Name.Match("Noggi") Then
			contHeight = cont.Geometry.GetParameterDouble("height")
			contWidth = cont.Geometry.GetParameterDouble("width")
		End If
	End If	
end sub


sub OnExecPerField()
	UpdateBaseNoggieHeight()
end sub
