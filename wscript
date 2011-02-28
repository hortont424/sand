#!/usr/bin/env python

from waflib.Configure import conf
import platform
import os

APPNAME = 'sand'
VERSION = '0.0'
UNAME = platform.uname()[0]

def options(opt):
    opt.tool_options('compiler_cxx compiler_c')

def configure(conf):
    conf.check_tool('compiler_cxx compiler_c')

    # if windows, add external/windows to the path

    conf.find_program('protoc')

    if UNAME is not "Windows":
        conf.pkgconfig_check()

    conf.library_check()

@conf
def pkgconfig_check(conf):
    libraries = (
        ("libglog", "glog", None),
        ("protobuf", "protobuf", None),
        ("cairo", "cairo", None)
    )

    for package, uselib, version in libraries:
        conf.check_cfg(package=package, uselib_store=uselib,
                       mandatory=True, version=version,
                       args='--cflags --libs')

@conf
def library_check(conf):
    if UNAME == "Darwin":
        libraries = ()
    elif UNAME == "Windows":
        libraries = ("opengl32", "glog", "protobuf")
    else:
        libraries = ("GL", "GLU")

    for library in libraries:
        conf.check_cc(lib=library)

def build(bld):
    bld.add_subdirs("src external/tinythread external/glfw external/soil")
    bld.add_group()

def run(ctx):
    if os.system("./waf build") is 0:
        return os.system("GLOG_logtostderr=1 ./build/src/game")

def db(ctx):
    if os.system("./waf build") is 0:
        return os.system("GLOG_logtostderr=1 gdb ./build/src/game")
