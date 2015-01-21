VERSION = "0.1.0"
LICENSE_URL = "https://raw.github.com/bvanderveen/httpmachine/HEAD/LICENSE.txt"
PROJECT_URL = "https://github.com/bvanderveen/httpmachine"
PROJECT_FILES = FileList["**/*.csproj"]

CONFIGURATION = "Release"
BUILD_DIR = File.expand_path("build")
OUTPUT_DIR = "#{BUILD_DIR}/out"
BIN_DIR = "#{BUILD_DIR}/bin"
NUGET_DIR = "#{BUILD_DIR}/nug"

require 'albacore'

def is_nix
  !RUBY_PLATFORM.match("linux|darwin").nil?
end

def invoke_runtime(cmd)
  command = cmd
  if is_nix()
    command = "mono --runtime=v4.0 #{cmd}"
  end
  command
end

def load_xml(input)
  input_file = File.new(input)
  xml = REXML::Document.new input_file
  input_file.close
  return xml
end

def transform_xml(input, output)
  xml = load_xml(input)
  
  yield xml
  
  output_file = File.open(output, "w")
  formatter = REXML::Formatters::Default.new()
  formatter.write(xml, output_file)
  output_file.close
end

def nuspec_for_project(project_file)
  project_file.chomp(".csproj").concat(".nuspec")
end

def load_nuspec_info(project_file)
  # get description, authors, copyright
  x = load_xml(nuspec_for_project(project_file))
  
  {
    "description" => x.root.elements["metadata/desciption"].text,
    "authors" => x.root.elements["metadata/authors"].text,
    "copyright" => x.root.elements["metadata/copyright"].text
  }
end

def update_assemblyinfo(project_file, version)
  begin
    nuspec_info = load_nuspec_info(project_file)
  rescue
    nuspec_info = nil
  end
  
  project_name = File.basename(project_file).chomp(".csproj")
  output_file = File.join(File.dirname(project_file), "Properties", "AssemblyInfo.cs")

  a = AssemblyInfo.new

  unless nuspec_info.nil?
    a.company_name = nuspec_info["authors"]
    a.copyright = nuspec_info["copyright"]
    a.description = nuspec_info["description"]
  end
  
  a.version = a.file_version = version
  a.product_name = a.title = project_name
  a.namespaces "System.Runtime.CompilerServices"
  a.custom_attributes :InternalsVisibleTo => "#{project_name}.Tests"
  a.output_file = output_file
  a.execute
end

def build_nuspec(project_file)  
  input_nuspec = nuspec_for_project(project_file)
  
  return unless File.exists? input_nuspec
  
  project_name = File.basename(project_file).chomp(".csproj")
  nuget_dir = "#{NUGET_DIR}/#{project_name}"
  FileUtils.mkdir nuget_dir

  output_nuspec = "#{nuget_dir}/#{project_name}.nuspec"

  transform_xml input_nuspec, output_nuspec do |x|
    x.root.elements["metadata/id"].text = project_name
    x.root.elements["metadata/version"].text = VERSION
    x.root.elements["metadata/owners"].text = x.root.elements["metadata/authors"].text
    x.root.elements["metadata/licenseUrl"].text = LICENSE_URL
    x.root.elements["metadata/projectUrl"].text = PROJECT_URL
  end

  nuget_lib_dir = "#{nuget_dir}/lib"
  FileUtils.mkdir nuget_lib_dir
  FileUtils.cp_r FileList["#{BIN_DIR}/#{project_name}{.dll,.pdb}"], nuget_lib_dir

  nuget = NuGetPack.new
  nuget.command = "tools/NuGet.exe"
  nuget.nuspec = output_nuspec
  nuget.output = BUILD_DIR
  #using base_folder throws as there are two options that begin with b in nuget 1.4
  nuget.parameters = "-Symbols", "-BasePath \"#{nuget_dir}\""
  nuget.execute
end

def ragel()
    sh "cd src/HttpMachine; ragel rl/HttpParser.cs.rl -A -o HttpParser.cs"
end

task :default => [:build]

msbuild :build_msbuild do |b|
  b.properties :configuration => CONFIGURATION, "OutputPath" => OUTPUT_DIR
  b.targets :Build
  b.solution = "HttpMachine.sln"
end

xbuild :build_xbuild do |b|
  b.properties :configuration => CONFIGURATION, "OutputPath" => OUTPUT_DIR
  b.targets :Build
  b.solution = "HttpMachine.sln"
end

task :build => :clean do
  ragel()
  build_task = is_nix() ? "build_xbuild" : "build_msbuild"
  Rake::Task[build_task].invoke
end

task :test => :build do
  nunit = invoke_runtime("packages/NUnit.2.5.10.11092/tools/nunit-console.exe")
  
  PROJECT_FILES
    .reject { |f| not f.include? ".Tests" }
    .map { |project_file| File.basename(project_file).chomp(".csproj") }
    .each { |project_name| sh "#{nunit} -labels #{OUTPUT_DIR}/#{project_name}.dll" }
end

task :binaries => [:build] do
  Dir.mkdir(BIN_DIR)
  binaries = FileList["#{OUTPUT_DIR}/*.dll", "#{OUTPUT_DIR}/*.pdb"]
    .exclude(/nunit/)
    .exclude(/.Tests/)
    .exclude(/.exe/)

  FileUtils.cp_r binaries, BIN_DIR
end

task :dist_nuget => [:binaries] do
  if is_nix()
    puts "Not running on Windows, skipping NuGet package creation."
  else 
    Dir.mkdir(NUGET_DIR)
    
    PROJECT_FILES.each do |p| 
      build_nuspec(p)
    end
  end
end

zip :dist_zip => [:binaries] do |z|
  z.directories_to_zip BIN_DIR
  z.output_file = "gate-#{VERSION}.zip"
  z.output_path = BUILD_DIR
end

task :dist => [:dist_nuget, :dist_zip] do
end

task :clean do
  FileUtils.rm_rf BUILD_DIR
  FileUtils.rm_rf FileList["src/**/obj", "src/**/bin"]
end
