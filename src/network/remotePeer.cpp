#include "remotePeer.h"

#include "handshakes.pb.h"

RemotePeer::RemotePeer(int writeSock, int readSock)
{
    this->writeSock = writeSock;
    this->readSock = readSock;
    this->name = NULL;
}

void RemotePeer::UpdateName(const char * name)
{
    LOG(INFO) << "RemotePeer::UpdateName() " << name;

    this->name = strdup(name);
}

const char * RemotePeer::GetName()
{
    return name;
}

void RemotePeer::ProcessMessage(SandMessage msg)
{
    if(msg.type() == SandMessage_MessageType_NAME_UPDATE)
    {
        UpdateName(msg.nameupdate().name().c_str());
    }

}