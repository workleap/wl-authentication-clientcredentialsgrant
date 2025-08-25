# Workleap.Authentication.ClientCredentialsGrant

OAuth 2.0 Client Credentials Grant authentication library for .NET, providing both client-side HTTP authentication and server-side ASP.NET Core JWT authorization. This is a NuGet package library project that builds two main packages for client and server authentication scenarios.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Setup
Install the exact .NET SDK version required by the project:
```bash
# Install .NET SDK 9.0.304 (required by global.json)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 9.0.304

# Install .NET 8.0 runtime (required for running tests)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 8.0.8 --runtime dotnet

# Install ASP.NET Core 8.0 runtime (required for integration tests)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --version 8.0.8 --runtime aspnetcore

# Add to PATH
export PATH="$HOME/.dotnet:$PATH"
```

### Build Commands
Navigate to the `src` directory for all build operations:
```bash
cd src

# Clean solution
dotnet clean

# Build (Debug configuration - RECOMMENDED)
dotnet build -c Debug
# Build time: ~5 seconds. NEVER CANCEL. Set timeout to 30+ seconds.

# Build (Release configuration - HAS ISSUES)
# DO NOT USE: dotnet build -c Release fails due to GitVersion requiring Git repository context
# The Release build fails with GitVersion.MsBuild errors in CI/local environments
```

### Test Commands
```bash
cd src

# Run all unit tests (after Debug build)
dotnet test -c Debug --no-build --verbosity normal
# Test time: ~63 seconds. NEVER CANCEL. Set timeout to 120+ seconds.

# Run tests with detailed output
dotnet test -c Debug --no-build --logger "console;verbosity=detailed"
```

### PowerShell Build Script
```bash
# The Build.ps1 script has known issues with GitVersion in non-CI environments
pwsh ./Build.ps1
# This command typically FAILS with GitVersion errors unless run in CI with proper Git context
# Expected error: "dotnet --roll-forward Major gitversion.dll exited with code 1"
# Build time when working: ~6 minutes. NEVER CANCEL. Set timeout to 10+ minutes.
```

## Project Structure

### Key Projects
- **Workleap.Extensions.Http.Authentication.ClientCredentialsGrant**: Client-side HTTP authentication library (.NET Standard 2.0)
- **Workleap.AspNetCore.Authentication.ClientCredentialsGrant**: Server-side ASP.NET Core JWT authorization library (ASP.NET Core 6.0+)
- **Workleap.Authentication.ClientCredentialsGrant.Tests**: Unit tests (net8.0)
- **tests/WebApi.OpenAPI.SystemTest**: Integration test/demo web API (net8.0)

### Important Files
- `global.json`: Specifies .NET SDK version 9.0.304
- `Build.ps1`: PowerShell build script (has GitVersion issues)
- `src/Directory.Build.props`: Shared MSBuild properties
- `src/*.snk`: Strong name key file for assembly signing

## Validation

### Manual Testing Scenarios
Always manually validate authentication functionality after making changes:

1. **Build and run the demo web API**:
   ```bash
   cd src/tests/WebApi.OpenAPI.SystemTest
   dotnet run
   # Access https://localhost:7219 or http://localhost:5241
   # Check Swagger UI at /swagger
   ```

2. **Test authentication endpoints**:
   - `/controller-allow-anonymous-override` - Should allow anonymous access
   - `/controller-requires-permission` - Should require authentication
   - Check OpenAPI security definitions in Swagger UI

3. **Validate OpenAPI generation**:
   ```bash
   cd src/tests/WebApi.OpenAPI.SystemTest
   dotnet build
   # OpenAPI document is generated and validated during build
   # Check tests/expected-openapi-document.yaml for expected output
   ```

### Code Quality Checks
Always run these before committing changes:
```bash
cd src

# The project uses Workleap.DotNet.CodingStandards for linting
# Linting is automatically applied during build
dotnet build -c Debug  # Includes automatic code analysis

# Check for API breaking changes
# Public APIs are tracked in PublicAPI.Shipped.txt and PublicAPI.Unshipped.txt files
# The build will fail if public API changes are not properly documented
```

## Common Tasks

### Adding New Features
1. **Client-side features**: Modify `Workleap.Extensions.Http.Authentication.ClientCredentialsGrant`
2. **Server-side features**: Modify `Workleap.AspNetCore.Authentication.ClientCredentialsGrant`
3. **Tests**: Add to `Workleap.Authentication.ClientCredentialsGrant.Tests`
4. **Integration testing**: Use `WebApi.OpenAPI.SystemTest` to verify end-to-end scenarios

### Working with Public APIs
- Always update `PublicAPI.Unshipped.txt` when adding new public APIs
- Move entries from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt` when releasing
- The build will fail if public API changes are not documented

### Release Process
- Preview packages are automatically published on main branch commits
- Stable releases are created by manually tagging with semantic version (e.g., `v1.2.3`)
- Uses GitVersion for automatic versioning (requires proper Git context)

## Known Issues and Limitations

### Build Issues
- **Release builds fail** in local/non-CI environments due to GitVersion requiring Git repository context
- **PowerShell Build.ps1 fails** for the same GitVersion reasons
- **Workaround**: Use Debug builds for local development and testing

### CI/CD Dependencies
- Project requires access to private NuGet feeds in CI
- Uses Azure Key Vault for secure dependency access
- Local builds work without these dependencies

### Runtime Requirements
- Tests require both .NET 8.0 runtime and ASP.NET Core 8.0 runtime
- Main libraries target .NET Standard 2.0 and ASP.NET Core 6.0+
- OpenAPI integration requires Swashbuckle.AspNetCore

## Repository Root Contents
```
.
..
.editorconfig
.git
.gitattributes  
.github/
.gitignore
Build.ps1
CODEOWNERS
CONTRIBUTING.md
GitVersion.yml
LICENSE
README.md
SECURITY.md
global.json
renovate.json
src/
```

## Common Command Outputs

### dotnet --version
```
9.0.304
```

### Directory listing: src/
```
.editorconfig
Directory.Build.props
Workleap.AspNetCore.Authentication.ClientCredentialsGrant/
Workleap.Authentication.ClientCredentialsGrant.Tests/
Workleap.Authentication.ClientCredentialsGrant.sln
Workleap.Authentication.ClientCredentialsGrant.snk
Workleap.Extensions.Http.Authentication.ClientCredentialsGrant/
tests/
```

### Successful test output summary
```
Test summary: total: 76, failed: 0, succeeded: 76, skipped: 0, duration: 63.0s
Build succeeded in 63.9s
```

### Demo API endpoints validation
```bash
# Test anonymous endpoint (should return 200)
curl -i http://localhost:5241/controller-allow-anonymous-override -X POST
# Expected: HTTP/1.1 200 OK, "Hello World!"

# Test protected endpoint (should return 401)  
curl -i http://localhost:5241/controller-requires-permission -X POST
# Expected: HTTP/1.1 401 Unauthorized, WWW-Authenticate: Bearer
```