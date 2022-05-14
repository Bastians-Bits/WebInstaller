#/bin/bash

INSTALL_DIR=/Users/$(whoami)/Documents/webinstaller_test
INSTALL_URL=http://localhost:8080/installer/linux
FILE_DIR=http://localhost:8080/files/zip

## Move to target directory
if [ ! -d ${INSTALL_DIR} ]; then 
  mkdir -p ${INSTALL_DIR}
  if [ $? != 0 ]; then
    echo "Failed to create installation directory"
    exit 1
  fi
fi 
pushd ${INSTALL_DIR}

## Create or Update the installation file
if [[ "$1" != "no-update" ]]; then
  # cUrl has extremely limited possibility. To get the filename from the server, we first have to download it
  INSTALL_NAME=$(curl -O -J -L -v ${INSTALL_URL} 2>&1 | grep "Content-Disposition" | grep -Eo 'filename=[^;]+' -)
  if [ "${INSTALL_NAME}" == "" ]; then echo "Failed to reach the sever"; exit 1; fi
  if [ -f ./${INSTALL_NAME} ]; then rm ./${INSTALL_NAME}; fi
  INSTALL_NAME=$(curl -O -J -L -v ${INSTALL_URL} 2>&1 | grep "Content-Disposition" | grep -Eo 'filename=[^;]+' -)
  if [ -f ./${INSTALL_NAME} ]; then echo "Failed to download the installation file"; exit 1; fi
fi

## Clean up /tmp
if [ -f /tmp/changes.zip ]; then rm /tmp/changes.zip; fi
if [ -d /tmp/changes ]; then rm -rf /tmp/changes; fi
  
if [ -f ./manifest.json ]; then
  ## Send the manifest and get changes
  curl -X GET http://localhost:8080/files/zip \
    -H 'Content-Type: application/json' \
    -d $(cat ./manifest.json) \
    -o /tmp/changes.zip
else
  ## Send an empty body and get everything
  curl -X GET http://localhost:8080/files/zip \
    -H 'Content-Type: application/json' \
    -d '{ "files": [] }' \
    -o /tmp/changes.zip
fi

unzip /tmp/changes.zip -d /tmp/changes