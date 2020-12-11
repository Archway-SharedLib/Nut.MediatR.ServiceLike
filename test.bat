REM dotnet tool install -g dotnet-reportgenerator-globaltool

REM Test Behaviors
dotnet test ./test/Nut.MediatR.Behaviors.Test/Nut.MediatR.Behaviors.Test.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=..\..\Nut.MediatR.Behaviors.coverage.xml
REM Test Behaviors.FluentValidation
dotnet test ./test/Nut.MediatR.Behaviors.FluentValidation.Test/Nut.MediatR.Behaviors.FluentValidation.Test.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=..\..\Nut.MediatR.Behaviors.FluentValidation.coverage.xml
REM Test ServiceLike
dotnet test ./test/Nut.MediatR.ServiceLike.Test/Nut.MediatR.ServiceLike.Test.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=..\..\Nut.MediatR.ServiceLike.coverage.xml
REM Test ServiceLike.DependencyInjection
dotnet test ./test/Nut.MediatR.ServiceLike.DependencyInjection.Test/Nut.MediatR.ServiceLike.DependencyInjection.Test.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=..\..\Nut.MediatR.ServiceLike.DependencyInjection.coverage.xml /p:Include="[Nut.MediatR.ServiceLike.DependencyInjection*]*"


reportgenerator "-reports:.\*.coverage.xml" "-targetdir:coveragereport" -reporttypes:Html
