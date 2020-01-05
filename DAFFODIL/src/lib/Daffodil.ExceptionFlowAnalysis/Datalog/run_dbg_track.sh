#!/bin/bash

rm -f dbg_track_run.out

z3 DbgTrack.datalog > dbg_track.out 2>&1
python3 parse_z3_out.py DbgTrack.datalog dbg_track.out >> dbg_track_run.out 2>&1
