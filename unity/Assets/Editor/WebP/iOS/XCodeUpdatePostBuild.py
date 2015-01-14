import os
import uuid
import sys
import re
import hashlib
import logging

def main(argv = sys.argv, pluginName = 'snappy-unity3d'):
    unityRootProject = os.getcwd()
    sys.stdout = Logger(unityRootProject + '/Snappy_XcodeUpdatePostBuild.log')
    if argv is None or len(argv) < 3:
        print 'Exiting: Incorrect number of arguments'
        print ', '.join(map(str, argv))
        return 2
    if 'iPhone' not in argv[2]:
        print 'Exiting: PostprocessBuildPlayer for Unity will only run for iPhone projects.'
        return 2
    projectPath = sys.argv[1]
    print '--- Updating Unity-iPhone.xcodeproj/project.pbxproj for snappy-unity3d integration ---'
    try:
        projectFile = projectPath + '/Unity-iPhone.xcodeproj/project.pbxproj'
        pluginPath = unityRootProject + '/Assets/Editor/Snappy/iOS/src/'
        p = XcodeProject(projectFile)
        p.projectPath = projectPath
        print 'Adding group "snappy-unity3d"'
        group = p.addGroup('snappy-unity3d')
        print 'Enabling Obj-C exceptions'
        p.findAndReplace('GCC_ENABLE_OBJC_EXCEPTIONS = NO;', 'GCC_ENABLE_OBJC_EXCEPTIONS = YES;')
       
        files = p.getFiles(pluginPath)
        for f in files:
            ext = os.path.splitext(f['name'])[1]
            if ext in ('.m', '.mm', '.c', '.cc'):
                fileHash = p.addFileReference(f['name'], f['path'], '"<group>"', pluginName)
                if fileHash is not None:
                    print 'adding build file: ' + f['name']
                    buildFileHash = p.addBuildFile(f['name'], fileHash)
                    p.addFileToBuildPhase(f['name'], buildFileHash, 'Sources')
            elif ext in ('.h',):
                fileHash = p.addFileReference(f['name'], f['path'], '"<group>"', pluginName)
                if fileHash is not None:
                    print 'adding file ref: ' + f['name']
            elif ext == '.a':
                fileHash = p.addFileReference(f['name'], f['path'], '"<group>"', 'Frameworks')
                if fileHash is not None:
                    print 'adding static library: ' + f['name']
                    buildFileHash = p.addBuildFile(f['name'], fileHash)
                    p.addFileToBuildPhase(f['name'], buildFileHash, 'Frameworks')
                    p.addLibrarySearchPath(pluginPath, pluginName)
            elif ext == '.patch':
                print 'patching with file: ' + f['name']
                fileToPatch = f['name'].replace(' ', '')[:-6]
                pathOfTarget = projectPath + '/Classes/' + fileToPatch
                command = 'patch --no-backup-if-mismatch -lNs "%s" "%s"' % (pathOfTarget, f['path'])
                os.system(command)
            elif ext == '.framework':
                print 'adding custom framework: ' + f['name']
                fileHash = p.addFileReference(f['name'], f['path'], '"<group>"', pluginName)
                if fileHash is not None:
                    buildFileHash = p.addBuildFile(f['name'], fileHash)
                    p.addFileToBuildPhase(f['name'], buildFileHash, 'Frameworks')
                    p.addFrameworkSearchPath(pluginPath, pluginName)
            elif ext == '.meta':
                print 'skipping meta file: ' + f['name']
            elif ext not in '.DS_Store':
                fileHash = p.addFileReference(f['name'], f['path'], '"<group>"', pluginName)
                if fileHash is not None:
                    print 'adding default resource file: ' + f['name']
                    buildFileHash = p.addBuildFile(f['name'], fileHash)
                    p.addFileToBuildPhase(f['name'], buildFileHash, 'Resources')

        p.save()
    except Exception as e:
        print 'Failed with error: %s' % e
        return 1

    print '--- Finished snappy-unity3d integration ---'


class XcodeProject(object):

    def __init__(self, filename):
        print 'Initializing XcodeProject...'
        self.static_libs = []
        self.projectFile = os.path.expanduser(filename)
        self.data = open(self.projectFile).read()
        target = re.escape('Unity-iPhone')
        result = re.search('[A-Z0-9]+ \\/\\* ' + target + ' \\*\\/ = \\{\n[\\s\t]+isa = PBXNativeTarget;(?:.|\n)+?buildConfigurationList = ([A-Z0-9]+) \\/\\* Build configuration list for PBXNativeTarget "' + target + '" \\*\\/;', self.data)
        if result:
            self.configListUuid = result.groups()[0]
            print 'Found UUID for the PBXNativeTarget: %s' % self.configListUuid
        else:
            raise Exception('XcodeProject: Could not find configurationList')
        match = re.search(re.escape(self.configListUuid) + ' \\/\\* Build configuration list for PBXNativeTarget "' + target + '" \\*\\/ = \\{\n[\\s\t]+isa = XCConfigurationList;\n[\\s\t]+buildConfigurations = \\(\n((?:.|\n)+?)\\);', self.data)
        if not match:
            raise Exception('Failed to get configuration list')
        configurationList = match.groups()[0]
        self.configurations = re.findall('[\\s\t]+([A-Z0-9]+) \\/\\* (.+) \\*\\/,\n', configurationList)
        result = re.search('([A-Z0-9]+) \\/\\* ' + target + ' \\*\\/ = {\n[\\s\t]+isa = PBXNativeTarget;(?:.|\n)+?buildPhases = \\(\n((?:.|\n)+?)\\);', self.data)
        if not result:
            print 'Unable to find the build phases'
            raise Exception('Failed to get build phases')
        buildPhases = result.groups()[1]
        self.buildPhases = {}
        for phase in ['Resources',
         'Frameworks',
         'CopyFiles',
         'Sources']:
            match = re.search('([A-Z0-9]+) \\/\\* ' + phase + ' \\*\\/', buildPhases)
            if match:
                self.buildPhases[phase] = match.groups()[0]
            else:
                self.buildPhases[phase] = None

        print 'Build phases:\n\t%s' % self.buildPhases

    def save(self):
        handle = open(self.projectFile, 'w')
        handle.write(self.data)
        handle.close()

    def findAndReplace(self, search, replace):
        self.data = self.data.replace(search, replace)

    def typeForFile(self, file):
        ext = os.path.splitext(file)[1].lower()
        if ext == '.mm':
            return 'sourcecode.cpp.objcpp'
        if ext == '.cpp':
            return 'sourcecode.cpp.cpp'
        if ext == '.cc':
            return 'sourcecode.cpp.cpp'
        if ext == '.h':
            return 'sourcecode.c.h'
        if ext == '.m':
            return 'sourcecode.c.objc'
        if ext == '.c':
            return 'sourcecode.c.c'
        if ext == '.framework':
            return 'wrapper.framework'
        if ext == '.png':
            return 'image.png'
        if ext in ('.jpg', '.jpeg'):
            return 'image.jpg'
        if ext == '.a':
            return 'archive.ar'
        if ext == '.bundle':
            return '"wrapper.plug-in"'
        if ext == '.xib':
            return 'file.xib'
        if ext == '.dylib':
            return '"compiled.mach-o.dylib"'
        if ext == '.sql':
            return 'file'
        if ext == '.json':
            return 'text'
        if ext == '.zip':
            return 'archive.zip'
        if ext == '.html':
            return 'text.html'
        if ext == '.xcdatamodel':
            return 'wrapper.xcdatamodel'
        if ext == '.framework':
            return 'wrapper.framework'
        print 'could not find wrapper type for file: ' + file
        return 'text'

    def uuid(self):
        genid = uuid.uuid4().__str__().upper().replace('-', '')
        genid = genid[0:24]
        return genid

    def getFiles(self, path):
        everything = os.listdir(path)
        files = []
        for item in everything:
            fullItemPath = os.path.join(path, item)
            if item[0] == '.' and item.endswith('plist') and item.endswith('.meta') or item.endswith('.txt'):
                continue
            if os.path.isfile(fullItemPath):
                files.append({'path': fullItemPath,
                 'name': item})
                continue
            if os.path.isdir(fullItemPath):
                if item.endswith('.bundle'):
                    files.append({'path': fullItemPath,
                     'name': item})
                elif item.endswith('.xcdatamodel'):
                    files.append({'path': fullItemPath,
                     'name': item})
                elif item.endswith('.framework'):
                    files.append({'path': fullItemPath,
                     'name': item})
                else:
                    files.extend(self.getFiles(fullItemPath))

        return files

    def addGroup(self, group):
        match = re.search('\\/\\* Begin PBXGroup section \\*\\/\n((?:.|\n)+?)\\/\\* End PBXGroup section \\*\\/', self.data)
        section = match.groups()[0]
        match = re.search('name\\s=\\s(' + re.escape(group) + ');', section)
        if match:
            print 'group [' + group + '] already exists. not going to create it'
            return None
        print 'group [' + group + '] doesnt exist.\tcreating it now.'
        match = re.search('\\/\\* Begin PBXGroup section \\*\\/\n', self.data)
        if not match:
            print 'could not find PBSGroup section'
            return False
        uuid = self.uuid()
        newGroup = '\t\t%s /* %s */ = {\n\t\t\tisa = PBXGroup;\n\t\t\tchildren = (\n\t\t\t);\n\t\t\tname = %s;\n\t\t\tsourceTree = "<group>";\n\n\t\t};\n' % (uuid, group, group)
        self.data = self.data[:match.end()] + newGroup + self.data[match.end():]
        match = re.search('\\/\\* CustomTemplate \\*\\/ = \\{\n[\\s\t]+isa = PBXGroup;\n[\\s\t]+children = \\(\n', self.data)
        if not match:
            print 'Could not find CustomTemplate'
            raise Exception('Error: could not find CustomTemplate')
        pbxgroup = '\t\t\t\t' + uuid + ' /* ' + group + ' */,\n'
        self.data = self.data[:match.end()] + pbxgroup + self.data[match.end():]
        return uuid

    def addFileReference(self, name, path, sourceTree, group, makePathRelative = True):
        uuid = None
        wrapper = self.typeForFile(name)
        escapedName = re.escape(name)
        if makePathRelative:
            path = os.path.relpath(path, self.projectPath)
        match = re.search('\\/\\* Begin PBXGroup section \\*\\/\n((?:.|\n)+?)\\/\\* End PBXGroup section \\*\\/', self.data)
        section = match.groups()[0]
        fileMatch = re.search('\\/\\* ' + escapedName + ' \\*\\/', section)
        if fileMatch:
            print 'File reference already exists: ' + name
            return
        match = re.search('([A-Z0-9]+) \\/\\* ' + escapedName + ' \\*\\/ = \\{isa = PBXFileReference; lastKnownFileType = ' + wrapper + '; name = ' + escapedName + '; path = ' + re.escape(path) + ';', self.data)
        if match:
            print 'This file has already been added: ' + name
            uuid = match.groups()[0]
            return
        match = re.search('\\/\\* Begin PBXFileReference section \\*\\/\n', self.data)
        if not match:
            print 'Could not find the PBXFileReference section.'
            return False
        uuid = self.uuid()
        fileRef = '\t\t' + uuid + ' /* ' + name + ' */ = {isa = PBXFileReference; lastKnownFileType = ' + wrapper + '; name = "' + name + '"; path = "' + path + '"; sourceTree = ' + sourceTree + '; };\n'
        print 'Adding file reference %s' % name
        self.data = self.data[:match.end()] + fileRef + self.data[match.end():]
        self.addFileToGroup(name, uuid, group)
        return uuid

    def addFileToGroup(self, name, guid, group):
        match = re.search('\\/\\* ' + group + ' \\*\\/ = \\{\n[\\s\t]+isa = PBXGroup;\n[\\s\t]+children = \\(\n((?:.|\n)+?)\\);', self.data)
        if not match:
            print 'Could not find children of group: ' + group
            return False
        section, = match.groups()
        match = re.search(re.escape(guid), section)
        if match:
            print 'Group already contains file'
        else:
            match = re.search('\\/\\* ' + group + ' \\*\\/ = \\{\n[\\s\t]+isa = PBXGroup;\n[\\s\t]+children = \\(\n', self.data)
            if not match:
                print 'Could not find group: ' + group
                return False
            pbxgroup = '\t\t\t\t' + guid + ' /* ' + name + ' */,\n'
            print 'Adding file %s to group %s' % (name, group)
            self.data = self.data[:match.end()] + pbxgroup + self.data[match.end():]
        return True

    def addFileToBuildPhase(self, name, uuid, phase, addOnBottom = True):
        phaseUuid = self.buildPhases[phase]
        phaseMatch = re.search(re.escape(phaseUuid) + ' \\/\\* ' + re.escape(phase) + ' \\*\\/ = {(?:.|\n)+?files = \\(((?:.|\n)+?)\\);', self.data)
        if not phaseMatch:
            print 'Could not find phase: ' + phase
            return False
        files = phaseMatch.groups()[0]
        match = re.search(re.escape(uuid), files)
        if match:
            print 'File %s already exists in phase %s' % (name, phase)
        else:
            match = re.search(re.escape(phaseUuid) + ' \\/\\* ' + phase + ' \\*\\/ = {(?:.|\n)+?files = \\(\n', self.data)
            if not match:
                print 'Could not locate files for phase: ' + phase
                return False
            match = re.search(re.escape(phaseUuid) + ' \\/\\* ' + phase + ' \\*\\/ = {(?:.|\n)+?files = \\(((?:.|\n)+?)\\);', self.data)
            phaseString = '\t' + uuid + ' /* ' + name + ' in ' + phase + ' */,\n\t\t\t\t);'
            endIndex = match.end() - 2
            print 'Adding %s to build phase %s' % (name, phase)
            self.data = self.data[:endIndex] + phaseString + self.data[endIndex + 2:]
        return True

    def addBuildFile(self, name, fileHash, isWeakFramework = False):
        match = re.search('\\/\\* Begin PBXBuildFile section \\*\\/\n((?:.|\n)+?)\\/\\* End PBXBuildFile section \\*\\/', self.data)
        if not match:
            print 'No PBXBuildFile section found.'
            return None
        section = match.groups()[0]
        match = re.search('([A-Z0-9]+).+?fileRef = ' + re.escape(fileHash), section)
        if match:
            uuid = match.groups()[0]
            return uuid
        match = re.search('\\/\\* Begin PBXBuildFile section \\*\\/\n', self.data)
        uuid = self.uuid()
        weak = ''
        if isWeakFramework:
            print 'Weak linking framework: ' + name
            weak = ' settings = {ATTRIBUTES = (Weak, ); }; '
        buildFile = '\t\t' + uuid + ' /* ' + name + ' in Frameworks */ = {isa = PBXBuildFile; fileRef = ' + fileHash + ' /* ' + name + ' */;' + weak + ' };\n'
        print 'Adding build file %s' % name
        self.data = self.data[:match.end()] + buildFile + self.data[match.end():]
        return uuid

    def addLinkerFlag(self, flag):
        matches = re.findall('OTHER_LDFLAGS = \\(([^)]*)\\);', self.data)
        for m in matches:
            newValue = m
            if len(newValue) > 0:
                if not newValue.strip().endswith(','):
                    newValue = m + ','
            newValue = newValue + '"' + flag + '"'
            updatedLinker = 'OTHER_LDFLAGS = ( ' + newValue + ');'
            matchString = 'OTHER_LDFLAGS = \\(' + m + '\\);'
            match = re.search(matchString, self.data)
            if not match:
                print 'could not match linker flag for %s' % flag
                sys.exit('could not match linker flag after parsing')
            print 'Adding Linker Flag %s' % newValue
            self.data = self.data[:match.start()] + updatedLinker + self.data[match.end():]

        matches = re.findall('OTHER_LDFLAGS = "(.*?)";', self.data)
        for m in matches:
            newValue = m
            if len(newValue) > 0:
                newValue = m + ','
            newValue = newValue + flag
            updatedLinker = 'OTHER_LDFLAGS = "' + newValue + '";'
            matchString = 'OTHER_LDFLAGS = "' + m + '";'
            match = re.search(matchString, self.data)
            if not match:
                print 'could not match linker flag for %s' % flag
                sys.exit('could not match linker flag after parsing')
            print 'Adding Linker Flag %s' % newValue
            self.data = self.data[:match.start()] + updatedLinker + self.data[match.end():]

    def addLibrarySearchPath(self, pluginPath, pluginName):
        print 'adding library search path for folder: ' + pluginName
        newPiece = '\n\t\t\t\t\t"\\"' + pluginPath + '\\"/**",'
        print 'library search path: ' + pluginPath
        self.data = str.replace(self.data, 'LIBRARY_SEARCH_PATHS = (', 'LIBRARY_SEARCH_PATHS = (' + newPiece)

    def addFrameworkSearchPath(self, pluginPath, pluginName):
        print 'adding framework search path %s for folder: %s' % (pluginPath, pluginName)
        relativePluginPath = os.path.relpath(pluginPath, self.projectPath)
        newPiece = '\n\t\t\t\t\t"\\"$(SRCROOT)/' + relativePluginPath + '\\"",'
        if 'FRAMEWORK_SEARCH_PATHS' in self.data:
            self.data = str.replace(self.data, 'FRAMEWORK_SEARCH_PATHS = (', 'FRAMEWORK_SEARCH_PATHS = (' + newPiece)
        else:
            newPiece = 'FRAMEWORK_SEARCH_PATHS = (\n\t\t\t\t\t"$(inherited)",' + newPiece
            self.data = str.replace(self.data, 'LIBRARY_SEARCH_PATHS = (', newPiece + '\n\t\t\t\t);\n\t\t\t\tLIBRARY_SEARCH_PATHS = (')


class Logger(object):

    def __init__(self, filename = 'Snappy_XcodeUpdatePostBuild.log'):
        self.terminal = sys.stdout
        self.log = open(filename, 'wb')

    def write(self, message):
        self.terminal.write(message)
        self.log.write(message)


if __name__ == '__main__':
    sys.exit(main())
