FROM registry.access.redhat.com/ubi9/ubi:latest

# Install SDK and TPM tools
RUN dnf install -y dotnet-sdk-8.0 tpm2-tools && dnf clean all

WORKDIR /app

# Copy the project file and the source code
COPY TpmTestApp.csproj .
COPY Program.cs .

# Build and publish
RUN dotnet publish -c Release -o out

ENTRYPOINT ["dotnet", "out/TpmTestApp.dll"]
