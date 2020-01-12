#!/usr/bin/env bash
curdir=`pwd`
make -j 8 CCINC="-I ../gmp-6.1.0 -I include " CCLIB="-L ../gmp-6.1.0/installdir/lib -L lib "
g++ -std=c++11 -O2 -march=native -Wall -Wextra -Werror -I ./include -I ../gmp-6.1.0 -Wl,-rpath -Wl,$curdir/../gmp-6.1.0/installdir/lib -fopenmp wrapper.cpp ./lib/libdai.a -L ../gmp-6.1.0/installdir/lib -lgmp -lgmpxx -o wrapper
