FROM node:10
RUN npm install -g azurite@2.7.1

# Azure Table Storage Emulator
EXPOSE 10002

CMD azurite-table -l ./