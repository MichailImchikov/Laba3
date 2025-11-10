#pragma once
#include <iostream>
#include <fstream>
#include <string>
#include <vector>
#include <algorithm>
#include <set>
#include <stdexcept>
#include <random>
#include <cassert>
#include <unordered_set>
#include <sstream>
#include <chrono>
#include <future>
#include <thread>

inline int input() {
    int num;
    std::cin >> num;
    return num;
}

inline void output(int num) {
    std::cout << num << std::endl;
}

// uniform distribution in range [low_inc, high_inc]
inline int get_random_number(int low_inc, int high_inc) {
    static std::random_device dev;
    static std::mt19937 rng(dev());
    std::uniform_int_distribution<std::mt19937::result_type> dist(low_inc, high_inc);
    return dist(rng);
}

// low_inc <= first < second <= high_inc
inline std::pair<int, int> get_random_non_repeating_pair(int low_inc, int high_inc) {
    if (low_inc >= high_inc) throw std::invalid_argument("low_inc >= high_inc for pair");
    int rnd1 = get_random_number(low_inc, high_inc);
    int rnd2 = get_random_number(low_inc, high_inc - 1);
    if (rnd2 >= rnd1) rnd2++;
    else std::swap(rnd1, rnd2);
    return {rnd1, rnd2};
}

template <typename T>
inline std::vector<int> ordered_sort_asc(std::vector<T> const& values) {
    std::vector<int> indices(values.size());
    std::iota(std::begin(indices), std::end(indices), 0);

    std::sort(
        std::begin(indices), std::end(indices),
        [&](int a, int b) { return values[a] < values[b]; }
    );
    return indices;
}

template <typename T>
inline std::vector<int> ordered_sort_desc(std::vector<T> const& values) {
    std::vector<int> indices(values.size());
    std::iota(std::begin(indices), std::end(indices), 0);

    std::sort(
        std::begin(indices), std::end(indices),
        [&](int a, int b) { return values[a] > values[b]; }
    );
    return indices;
}

template <typename T>
inline void shuffle(std::vector<T>& values) {
    static auto rng = std::default_random_engine {};
    std::shuffle(std::begin(values), std::end(values), rng);
}

struct Permutation {
    std::vector<size_t> data;
    static bool check(const std::vector<size_t>& data) {
        std::set<size_t> check_set;
        for (auto elem : data) {
            if (check_set.count(elem) != 0 || elem >= data.size()) {
                return false;
            }
            check_set.insert(elem);
        }
        return true;
    }
    static Permutation make(std::vector<size_t> data) {
        if (!check(data)) throw std::invalid_argument("invalid permutation");
        return Permutation{ .data = data };
    }
    static Permutation make_identity(size_t size) {
        std::vector<size_t> data(size);
        std::iota(data.begin(), data.end(), 0);
        return make(data);
    }
};

template<typename Func>
bool execute_with_timeout(Func&& func, std::chrono::milliseconds timeout) {
    auto future = std::async(std::launch::async, std::forward<Func>(func));
    
    auto status = future.wait_for(timeout);

    if (status == std::future_status::ready) {
        return true; // Completed successfully
    } else {
        return false; // Timed out
    }
}