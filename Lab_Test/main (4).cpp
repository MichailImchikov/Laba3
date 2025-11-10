#include "lablib.h"
using Times = std::vector<std::vector<uint32_t>>; // [FROM] [TO]
using DirectiveTimes = std::vector<uint32_t>;
//#define DEBUG

static const double C1 = -17, C2 = 44, C3 = 46;

struct Leaf
{
    std::vector<size_t> verts;
    std::vector<bool> remaining;
    size_t t;
    size_t failed;
    // B and H are modified only when the remaining are valid
    size_t B;
    size_t H;
    std::string to_string(std::string desc = "") const
    {
        std::stringstream output;
        output << desc << " [ ";
        for (auto vert : verts)
        {
            output << vert << " ";
        }
        output << "] H " << H << " B " << B;
        return output.str();
    }
    void print(std::string desc = "") const
    {
        std::cout << to_string(desc) << "\n";
    }
    void debug_print(std::string desc = "") const
    {
#ifdef DEBUG
        print(desc);
#endif
    }
};

struct ASolver
{
    Times times;
    DirectiveTimes directive_times;
    size_t n;

    ASolver(Times times_, DirectiveTimes directive_times_)
    : times(times_), directive_times(directive_times_), n(times_.size())
    {
        // Здесь n = (n из описания алгоритма) + 1
        assert(n == directive_times.size());
        assert(n == times.front().size());
        // Для однозначности директивный срок 0 у начальной точки:
        assert(directive_times[0] == 0);
    }

    virtual size_t branch(const std::vector<Leaf>& leaves) const = 0;
    // If !!found, returns leaf with B penalty:
    virtual size_t calc_B(const Leaf& leaf, Leaf* found) const = 0;
    virtual size_t calc_H(const Leaf& leaf) const = 0;

    virtual Leaf update_leaf(Leaf leaf, size_t a) const
    {
        leaf.t = leaf.t + times[leaf.verts.back()][a];
        leaf.verts.push_back(a);
        if (leaf.t > directive_times[a]) leaf.failed++;
        leaf.remaining[a] = false;

        // EVERYTHING EXCEPT B and H IS INITIALIZED.

        leaf.B = calc_B(leaf, nullptr);
        leaf.H = calc_H(leaf);
        return leaf;
    }

    Leaf branch_and_bound(size_t* leaves_traversed = nullptr) const
    {
        // Шаг 1:
        Leaf root
        {
            .verts = {0},
            .remaining = std::vector<bool>(n, true),
            .t = 0,
            .failed = 0
        };
        root.H = calc_H(root);
        root.B = calc_B(root, nullptr);
        root.remaining[0] = false;
        std::vector<Leaf> leaves = { root };
        if (leaves_traversed) *leaves_traversed = 1;

        for (;;)
        {
            // Шаг 3:
            size_t leaf_id = branch(leaves);
            Leaf leaf = leaves[leaf_id];
            for (size_t a = 0; a < n; a++)
            {
                if (!leaf.remaining[a]) continue;
                leaves.push_back(update_leaf(leaf, a));
                (*leaves_traversed)++;
            }
            leaves[leaf_id].debug_print("Erase because picked this one");
            leaves.erase(leaves.begin() + leaf_id);

            auto get_min_B_leaf = [](const Leaf& a, const Leaf& b) -> bool
            {
                return a.B < b.B;
            };
            auto min_B_iter = std::min_element(leaves.begin(), leaves.end(), get_min_B_leaf);
            size_t min_B_id = min_B_iter - leaves.begin();
            size_t min_B = min_B_iter->B;
#ifdef DEBUG
            std::cout << "-- B = " << min_B << " --\n";
#endif 

            // Шаг 4:
            for (size_t i = 0; i < leaves.size(); i++)
            {
                // TODO: за один проход переносить влево элементы вместо erase.
                if (i == min_B_id)
                {
                    leaves[i].debug_print("Skip because current");
                    continue;
                }
                if (min_B <= leaves[i].H)
                {
                    leaves[i].debug_print("Erase because H too bad");
                    leaves.erase(leaves.begin() + i);
                    i--;
                    min_B_id--;
                }
            }

            // Шаг 2:
            if (leaves.size() == 1 && leaves.front().B == leaves.front().H)
            {
                Leaf leaf = leaves.front();
                Leaf output;
                calc_B(leaf, &output);
                return output;
            }

            for (auto leaf : leaves)
            {
                leaf.debug_print();
            }
        }
    }

};

struct BaseSolver : public ASolver
{
    BaseSolver(Times times_, DirectiveTimes directive_times_)
    : ASolver(times_, directive_times_)
    {

    }
    virtual size_t branch(const std::vector<Leaf>& leaves) const override
    {
        size_t min_B = (size_t)-1;
        size_t min_B_id = (size_t)-1;
        for (size_t i = 0; i < leaves.size(); i++)
        {
            if (leaves[i].verts.size() < n && leaves[i].B < min_B)
            {
                min_B = leaves[i].B;
                min_B_id = i;
            }
        }
        return min_B_id;
    }
    // If !!found, returns leaf with B penalty:
    virtual size_t calc_B(const Leaf& leaf, Leaf* found) const override
    {
        size_t dummy;
        if (found) *found = leaf;
        std::unordered_set<size_t> remaining;
        for (size_t i = 0; i < leaf.remaining.size(); i++)
        {
            if (leaf.remaining[i]) remaining.insert(i);
        }
        size_t t = leaf.t;
        size_t last_vert_id = leaf.verts.back();
        while (remaining.size() > 0)
        {
            size_t min_diff = (size_t)-1;
            size_t min_diff_id = (size_t)-1;
            for (size_t index : remaining)
            {
                size_t time_after = t + times[last_vert_id][index];
                if (time_after > directive_times[index]) continue;
                size_t diff = directive_times[index] - time_after;
                if (diff < min_diff)
                {
                    min_diff = diff;
                    min_diff_id = index;
                }
            }
            if (min_diff_id == (size_t)-1)
            {
                // All remaining verts are bad
                if (found)
                {
                    for (size_t index : remaining) *found = update_leaf(*found, index);
                }
                return leaf.failed + remaining.size();
            }
            if (found) *found = update_leaf(*found, min_diff_id);
            t += times[last_vert_id][min_diff_id];
            remaining.erase(min_diff_id);
            last_vert_id = min_diff_id;
        }
        // No verts remaining
        return leaf.failed;
    }
    virtual size_t calc_H(const Leaf& leaf) const override
    {
        size_t output = leaf.failed;
        for (size_t a = 0; a < n; a++)
        {
            if (!leaf.remaining[a]) continue;
            if (leaf.t + times[leaf.verts.back()][a] > directive_times[a]) output++;
        }
        return output;
    }
};

struct MySolver : public BaseSolver
{
    MySolver(Times times_, DirectiveTimes directive_times_)
    : BaseSolver(times_, directive_times_)
    { }
    size_t branch(const std::vector<Leaf>& leaves) const override
    {
        double min_score = __DBL_MAX__;
        size_t min_score_id = (size_t)-1;
        for (size_t i = 0; i < leaves.size(); i++)
        {
            double score = double(leaves[i].B - leaves[i].H) * C1 + double(n - leaves[i].verts.size()) * C2 + double(leaves[i].B) * C3;
            if (leaves[i].verts.size() < n && score < min_score)
            {
                min_score = score;
                min_score_id = i;
            }
        }
        return min_score_id;
    }
};

template <typename SolverT>
SolverT load(std::string file_name)
{
    std::ifstream stream(file_name);
    size_t n;
    stream >> n;
    DirectiveTimes directive_times(n + 1);
    for (size_t i = 0; i < n; i++)
    {
        stream >> directive_times[i + 1];
    }
    Times times = std::vector<std::vector<uint32_t>>(n + 1, std::vector<uint32_t>(n + 1));
    for (size_t i = 0; i <= n; i++)
    {
        for (size_t j = 0; j <= n; j++)
        {
            stream >> times[i][j];
        }
    }
    return SolverT{times, directive_times};
}

template <typename SolverT>
void get_measurements(const char* filename, std::chrono::microseconds* time, Leaf* leaf, size_t* leaves_traversed)
{
    using namespace std::chrono;
    auto solver = load<SolverT>(filename);
    auto start = high_resolution_clock::now();
    auto result = solver.branch_and_bound(leaves_traversed);
    auto stop = high_resolution_clock::now();
    auto duration = duration_cast<microseconds>(stop - start);
    if (leaf) *leaf = result;
    if (time) *time = duration;
}

int main(int argc, char** argv)
{
    using namespace std::chrono;
    double eff_time = 0;
    double eff_traversed = 0;
    for (int i = 1; i < argc; i++)
    {
        std::chrono::microseconds base_time, my_time;
        Leaf result;
        size_t base_leaves_traversed, my_leaves_traversed;
        get_measurements<BaseSolver>(argv[i], &base_time, &result, &base_leaves_traversed);
        get_measurements< MySolver >(argv[i], &my_time  , nullptr, &my_leaves_traversed  );
        eff_time += (double(base_time.count()) - double(my_time.count())) / base_time.count();
        eff_traversed += (double(base_leaves_traversed) - double(my_leaves_traversed)) / base_leaves_traversed;
        std::cout << argv[i] << ":" << result.to_string() << " R " << result.failed << ". base/my: Traversed " << base_leaves_traversed << "/" << my_leaves_traversed << " in " << base_time.count() << "/" << my_time.count() << "μs.)\n";
    }
    std::cout << "-------\n";
    std::cout << "eff_time " << (eff_time / (argc - 1)) << " eff_traversed " << (eff_traversed / (argc - 1)) << "\n";
}
