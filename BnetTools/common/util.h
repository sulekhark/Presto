#pragma once

#include <boost/functional/hash.hpp>
#include <boost/lexical_cast.hpp>
using boost::hash_range;
using boost::lexical_cast;

#include <chrono>
#include <ctime>
#include <iostream>
#include <vector>
using namespace std;

extern string nowstr();

#define __LOGSTR_FULL__ (nowstr() + " " + __FILE__ + ": " + lexical_cast<string>(__LINE__) + ": ")
#define __LOGSTR__ "" 

// https://stackoverflow.com/questions/20590656/error-for-hash-function-of-pair-of-ints
struct pair_hash {
    size_t operator()(const pair<int, int>& pr) const {
        // https://stackoverflow.com/questions/5889238/why-is-xor-the-default-way-to-combine-hashes
        return hash<int>()(pr.first) * 31 + hash<int>()(pr.second);
    }
};

template <typename T>
struct vector_hash {
    size_t operator()(const vector<T>& v) const {
        return hash_range(v.begin(), v.end());
    }
};

template <typename T>
ostream& operator<<(ostream& stream, const vector<T>& v) {
    for (size_t i = 0; i + 1 < v.size(); i++) {
        stream << v[i] << ", ";
    }

    if (v.size() > 0) {
        stream << v[v.size() - 1];
    }

    return stream;
}

template <typename T>
T setPop(unordered_set<T> *u) {
    T t = *u->begin();
    u->erase(u->begin());
    return t;
}
