class DisplayMeshData:
    character = ""
    child = ""
    position = ""
    angle = ""
    scale = ""

    def Reset(self):
        # Reset display mesh data - Remembers character name
        self.child = ""
        self.position = ""
        self.angle = ""
        self.scale = ""

    def GetReady(self):
        # Check if display mesh data is ready to be output as a line of code
        if self.character != "" and self.child != "" and self.position != "" and self.angle != "" and self.scale != "":
            return True
        return False

    def GetCode(self):
        return f"""displaySettings.AddCharacterDisplay("{self.character}", "{self.child}", {self.position}, {self.angle}, {self.scale});"""

    def SetCharacter(self, _character):
        # Refine string and set as character
        self.character = _character.strip().replace(':', '')

    def SetChild(self, _child):
        # Refine string and set as child
        self.child = _child.split('=')[1].strip().replace('"', '')

        # Check for trailing comma
        if self.child[-1] == ',':
            self.child = self.child[:-1]

    def SetPosition(self, _position):
        # Refine string and set as position
        self.position = _position.split('=')[1].strip()

        # Check for trailing comma
        if self.position[-1] == ',':
            self.position = self.position[:-1]

    def SetAngle(self, _angle):
        # Refine string and set as angle
        self.angle = _angle.split('=')[1].strip()

        # Check for trailing comma
        if self.angle[-1] == ',':
            self.angle = self.angle[:-1]

    def SetScale(self, _scale):
        # Refine string and set as scale
        self.scale = _scale.split('=')[1].strip()

        # Check for trailing comma
        if self.scale[-1] == ',':
            self.scale = self.scale[:-1]


# Open raw file
rawFile = open("raw.txt", "r").readlines()

# Output string
output = ""

# Store current display mesh data
currentData = DisplayMeshData()

# Cycle through lines
for line in rawFile:
    # Strip line
    stripped = line.strip()

    # Continue if empty
    if stripped == "":
        continue

    # Check if child line
    if "childName" in stripped:
        # Pass to display mesh data
        currentData.SetChild(stripped)

    # Check if position line
    elif "localPos" in stripped:
        # Pass to display mesh data
        currentData.SetPosition(stripped)

    # Check if angles line
    elif "localAngles" in stripped:
        # Pass to display mesh data
        currentData.SetAngle(stripped)

    # Check if scale line
    elif "localScale" in stripped:
        # Pass to display mesh data
        currentData.SetScale(stripped)

    # Otherwise assume character name
    else:
        # Pass to display mesh data
        currentData.SetCharacter(stripped)

    # Check if display mesh data is ready to output
    if currentData.GetReady():
        # Check if new line needed
        if output != "":
            # Add new line
            output += "\n"

        # Add to output
        output += currentData.GetCode()

        # Reset current data
        currentData.Reset()

# Output to file
open("output.txt", "w").write(output)
