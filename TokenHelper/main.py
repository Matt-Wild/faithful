# Open raw file
rawFile = open("raw.txt", "r").readlines()

# Output string
output = ""

# Cycle through lines
for line in rawFile:
    # Replace quotation with unicode
    line = line.replace('"', r'\u201C')

    # Replace new line
    line = line.replace('\n', r'\n')

    # Add to output string
    output += line

# Output to file
open("output.txt", "w").write(output)
