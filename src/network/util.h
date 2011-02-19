#include <sys/socket.h>
#include <arpa/inet.h>
#include <cstdlib>
#include <cstring>

char * sockaddrToString(const struct sockaddr * sa);