ARG MCRA_DEV_REGISTRY="localhost"
ARG DOCKER_IO_REPO="docker.io"

FROM ${MCRA_DEV_REGISTRY}/mcra-base:main AS build

# install dotnet sdk and dependencies in separate step
# we need: git, libgdiplus (png rendering) and dotnet SDK
RUN add-apt-repository -y "ppa:dotnet/backports" \
  && apt-get update -qq\
  && apt-get install -y --no-install-recommends\
    git-core\
    dotnet-sdk-9.0\
  && rm -rf /var/lib/apt/lists/* \
  && ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so

# stop git warnings about LF/CRLF line endings
RUN git config --global core.safecrlf false

# install dotnet tools
ENV PATH="/root/.dotnet/tools:${PATH}"
RUN dotnet tool install -g dotnet-t4
RUN dotnet tool install -g dotnet-coverage
RUN dotnet tool install -g dotnet-reportgenerator-globaltool

# command to build:
# podman build -f UnitTest.Dockerfile -v "$(pwd):/src" --build-arg "CACHE_DATE=$(date +%Y-%m-%d:%H:%M:%S)" -t mcra-core:unittest .
# run restore of csproj files in a separate folder within the container
WORKDIR /restr
# Create file GitInfo.ThisAssembly.cs, contents will be overwritten later
# RUN touch "GitInfo.ThisAssembly.cs"
COPY ["Shared/", "Shared/"]
# copy csproj and solution file
COPY ["**/*.csproj", "mcra-core.sln", "."]
# move csproj files to their respective subfolders based on file name
RUN for f in *.csproj; do mkdir "${f%.csproj}" && cp "$f" "${f%.csproj}" && rm "$f"; done
# Restore the solution
RUN dotnet restore "mcra-core.sln"

# /src is a mounted folder from the host containing the source code
WORKDIR /src

ARG MCRA_BUILD_CONFIG="Release"
ENV R_HOME=/usr/lib/R

## Invalidate cache to force build of following stages
ARG CACHE_DATE=empty_date_value
RUN echo CACHE_DATE = ${CACHE_DATE}

# build full solution
RUN dotnet build "mcra-core.sln" -c ${MCRA_BUILD_CONFIG} -p:UseAppHost=false

# run all unit test projects
ENV ASPNETCORE_ENVIRONMENT="docker"
# MCRA Core tests
RUN dotnet test ./MCRA.Utils.Test/bin/${MCRA_BUILD_CONFIG}/MCRA.Utils.Test.dll -v quiet --filter "(TestCategory!=Sandbox Tests)" -s "./.runsettings" -l "html;LogFileName=./MCRA.Utils.Test.html" --collect:"Code Coverage"
RUN dotnet test ./MCRA.General.Test/bin/${MCRA_BUILD_CONFIG}/MCRA.General.Test.dll -v quiet --filter "(TestCategory!=Sandbox Tests)" -s "./.runsettings" -l "html;LogFileName=./MCRA.General.Test.html" --collect:"Code Coverage"
RUN dotnet test ./MCRA.Data.Raw.Test/bin/${MCRA_BUILD_CONFIG}/MCRA.Data.Raw.Test.dll -v quiet --filter "(TestCategory!=Sandbox Tests)" -s "./.runsettings" -l "html;LogFileName=./MCRA.Data.Raw.Test.html" --collect:"Code Coverage"
RUN dotnet test ./MCRA.Data.Compiled.Tests/bin/${MCRA_BUILD_CONFIG}/MCRA.Data.Compiled.Tests.dll -v quiet --filter "(TestCategory!=Sandbox Tests)" -s "./.runsettings" -l "html;LogFileName=./MCRA.Data.Compiled.Tests.html" --collect:"Code Coverage"
RUN dotnet test ./MCRA.Data.Management.Tests/bin/${MCRA_BUILD_CONFIG}/MCRA.Data.Management.Tests.dll -v quiet --filter "(TestCategory!=Sandbox Tests)" -s "./.runsettings" -l "html;LogFileName=./MCRA.Data.Management.Tests.html" --collect:"Code Coverage"
RUN dotnet test ./MCRA.Simulation.Test/bin/${MCRA_BUILD_CONFIG}/MCRA.Simulation.Test.dll -v quiet --filter "(TestCategory!=Sandbox Tests)" -s "./.runsettings" -l "html;LogFileName=./MCRA.Simulation.Test.html" --collect:"Code Coverage"

# copy the /src/bin folder to /app/mcra-bin
RUN mkdir -p /app/mcra-bin && cp -r bin/* /app/mcra-bin

# Publish stage
FROM build as publish
ARG MCRA_BUILD_CONFIG="Release"
RUN dotnet publish "MCRA.Simulation.Commander/MCRA.Simulation.Commander.csproj" \
  -c ${MCRA_BUILD_CONFIG}\
  -o /app/publish\
  -p:UseAppHost=false\
  --no-build --no-restore

# final stage/image
# use a very small image for storing the build output files
# which doesn't need to run anything
FROM ${DOCKER_IO_REPO}/busybox as appstore

WORKDIR /apps
# Copy the MCRA binaries that are referenced in the web apps
COPY --from=build /app/mcra-bin ./mcra-bin/
# Copy the MCRA command line interface files (CLI)
COPY --from=publish /app/publish ./mcra-cli/
