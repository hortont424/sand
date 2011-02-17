/*
 * Copyright 2011 Tim Horton and Pete Schirmer. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY TIM HORTON "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL TIM HORTON OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "mdns.h"

// TODO: ifdef darwin?

#include <CoreServices/CoreServices.h>

void MDNSResponderCallback(CFNetServiceRef svc, CFStreamError * err, void * info)
{
    printf("CB!\n");
}

#pragma mark Public Functions

void MDNSResponderTick()
{
    CFRunLoopRunInMode(kCFRunLoopDefaultMode, 0, true);
}

void MDNSRegisterService(MDNSService * service)
{
    CFNetServiceRef svc;
    CFNetServiceClientContext * clientCtx;
    CFStringRef domain, serviceType, name;

    domain = CFStringCreateWithCString(NULL, "local", CFStringGetSystemEncoding());
    serviceType = CFStringCreateWithCString(NULL, service->type, CFStringGetSystemEncoding());
    name = CFStringCreateWithCString(NULL, service->name, CFStringGetSystemEncoding());

    svc = CFNetServiceCreate(NULL, domain, serviceType, name, service->port);

    if(!svc)
    {
        printf("CFNetServiceCreate() failed!\n");
        return;
    }

    clientCtx = (CFNetServiceClientContext *)calloc(1, sizeof(CFNetServiceClientContext));

    if(!CFNetServiceSetClient(svc, MDNSResponderCallback, clientCtx))
    {
        printf("CFNetServiceSetClient() failed!\n");
        return;
    }

    CFNetServiceScheduleWithRunLoop(svc, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);

    if(!CFNetServiceRegisterWithOptions(svc, 0, NULL))
    {
        printf("CFNetServiceRegisterWithOptions() failed!\n");
        return;
    }
}

std::vector<MDNSService *> MDNSBrowseService(MDNSService * service)
{

}