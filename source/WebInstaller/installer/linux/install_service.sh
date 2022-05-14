#/bin/bash

INSTALL_DIR=/Users/$(whoami)/Documents/webinstaller_test
INSTALL_URL=http://localhost:8080/installer/linux

if [[ "$1" != "no-update" ]]; then
  # cUrl has extremely limited possibility. To get the filename from the server, we first have to download it
  INSTALL_NAME=$(curl -O -J -L -v ${INSTALL_URL} 2>&1 | grep "Content-Disposition" | grep -Eo 'filename=[^;]+' -)
  if [ "${INSTALL_NAME}" == "" ]; then echo "Could not download installer file"; exit 1; fi
  if [ -f ./${INSTALL_NAME} ]; then rm ./${INSTALL_NAME}; fi
  INSTALL_NAME=$(curl -O -J -L -v ${INSTALL_URL} 2>&1 | grep "Content-Disposition" | grep -Eo 'filename=[^;]+' -)
fi
