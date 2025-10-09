# https://hub.docker.com/_/microsoft-dotnet
ARG MCRA_DEV_REGISTRY="localhost"
ARG MCRA_IMAGE_TAG="main"

FROM ${MCRA_DEV_REGISTRY}/mcra-core:${MCRA_IMAGE_TAG} AS mcracore

# final stage/image
FROM ${MCRA_DEV_REGISTRY}/mcra-base:main AS runtime

# install dotnet sdk and dependencies in separate step
# we need: git, libgdiplus (png rendering) and dotnet SDK
RUN add-apt-repository -y "ppa:dotnet/backports" \
  && apt-get update -qq\
  && apt-get install -y --no-install-recommends aspnetcore-runtime-9.0\
  && rm -rf /var/lib/apt/lists/* \
  && ln -s /usr/lib/x86_64-linux-gnu/libdl.so.2 /usr/lib/x86_64-linux-gnu/libdl.so

ARG MCRA_ASPNETCORE_ENVIRONMENT="Release"

WORKDIR /app
COPY --from=mcracore /apps/mcra-cli ./

# running:
# podman run --rm -it -v /mnt/x/path:/data mcra-cli:v10
ENV R_HOME=/usr/lib/R
ENV RHomePath=/usr/lib/R
ENV PythonPath=/usr/lib/python3.12/config-3.12-x86_64-linux-gnu/libpython3.12.so
ENV ProjectOutputBaseFolder=/data/outputs
ENV ASPNETCORE_ENVIRONMENT=${MCRA_ASPNETCORE_ENVIRONMENT}
ENTRYPOINT [ "dotnet", "mcra.dll" ]
