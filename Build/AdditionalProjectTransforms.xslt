<xsl:if test="/Input/Properties/DoGrpcGenerate = 'True'">
  <Target Name="BeforeBuild">
    <Exec 
      WorkingDirectory="$(ProjectDir)"
      Command="..\Grpc.Tools\windows_x86\protoc.exe --proto_path . --csharp_out Editor --grpc_out Editor Editor.proto --plugin=protoc-gen-grpc=../Grpc.Tools/windows_x86/grpc_csharp_plugin.exe" />
    <Exec 
      WorkingDirectory="$(ProjectDir)"
      Command="..\Grpc.Tools\windows_x86\protoc.exe --proto_path . --csharp_out ExtensionHost --grpc_out ExtensionHost ExtensionHost.proto --plugin=protoc-gen-grpc=../Grpc.Tools/windows_x86/grpc_csharp_plugin.exe" />
    <Exec 
      WorkingDirectory="$(ProjectDir)"
      Command="..\Grpc.Tools\windows_x86\protoc.exe --proto_path . --csharp_out GameHost --grpc_out GameHost GameHost.proto --plugin=protoc-gen-grpc=../Grpc.Tools/windows_x86/grpc_csharp_plugin.exe" />
  </Target>
</xsl:if>