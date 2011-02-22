#include "remotePeer.h"

RemotePeer::RemotePeer(int writeSock, int readSock)
{
    this->writeSock = writeSock;
    this->readSock = readSock;
    this->name = NULL;
}

void RemotePeer::UpdateName(const char * name)
{
    LOG(INFO) << "RemotePeer::UpdateName() " << name;

    this->name = name;
}

const char * RemotePeer::GetName()
{
    return name;
}
