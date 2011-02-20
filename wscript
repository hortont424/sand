#!/usr/bin/env python

from waflib.Configure import conf
import os

APPNAME = 'sand'
VERSION = '0.0'
UNAME = os.uname()[0]

def options(opt):
    opt.tool_options('compiler_cxx compiler_c')

def configure(conf):
    conf.check_tool('compiler_cxx compiler_c')
    conf.find_program('protoc')

    conf.pkgconfig_check()
    conf.library_check()

@conf
def pkgconfig_check(conf):
    libraries = (
        ("libglog", "glog", None),
        ("protobuf", "protobuf", None)
    )

    for package, uselib, version in libraries:
        conf.check_cfg(package=package, uselib_store=uselib,
                       mandatory=True, version=version,
                       args='--cflags --libs')

@conf
def library_check(conf):
    if UNAME == "Darwin":
        libraries = ("gc",)
    elif UNAME == "Windows":
        libraries = ("GL", "gc")
    else:
        libraries = ("GL", "gc")

    for library in libraries:
        conf.check_cc(lib=library)

def build(bld):
    bld.add_subdirs("src external/tinythread external/glfw")
    bld.add_group()

def run(ctx):
    if os.system("./waf build") is 0:
        return os.system("GLOG_logtostderr=1 ./build/src/game")

def db(ctx):
    if os.system("./waf build") is 0:
        return os.system("GLOG_logtostderr=1 gdb ./build/src/game")
